using AuthMicroservice.BusinessLogic.Interfaces.IServices;
using AuthMicroservice.DataAccess.IRepositories;
using AuthMicroservice.Shared.Dtos.Account;
using AuthMicroservice.DataAccess.Entites;

namespace AuthMicroservice.BusinessLogic.Services
{
    public class AuthenticationService(IUserRepository userRepository, ITokenService tokenService)
        : IAuthenticationService
    {
        public async Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password)
        {
            var user = await userRepository.FindByEmailAsync(email);
            if (user == null) throw new UnauthorizedAccessException("Invalid email!");

            var result = await userRepository.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded) throw new UnauthorizedAccessException("Email not found and/or password incorrect");

            var accessToken = await tokenService.CreateAccessToken(user); 
            var refreshToken = await tokenService.CreateRefreshToken(); 

            user.RefreshTokenHash = await tokenService.HashToken(refreshToken);
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userRepository.UpdateAsync(user);

            return (accessToken, refreshToken);
        }

        public async Task RegisterAsync(RegisterDto registerDto)
        {
            var refreshToken = await tokenService.CreateRefreshToken(); 
            var user = new UserEntity
            {
                UserName = registerDto.Email.Split('@')[0],
                Email = registerDto.Email,
                RefreshTokenHash = await tokenService.HashToken(refreshToken),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
            };

            var createdUser = await userRepository.CreateAsync(user, registerDto.Password);
            if (!createdUser.Succeeded) throw new InvalidOperationException("User creation failed");

            await userRepository.UpdateAsync(user);
        }
        

        public async Task<UserEntity> GetUserByIdAsync(string userId)
        {
            return await userRepository.FindByIdAsync(userId);
        }
    }
}
