using AuthMicroservice.DataAccess.Entites;
using AuthMicroservice.Shared.Dtos.Account;

namespace AuthMicroservice.BusinessLogic.Interfaces.IServices
{
    public interface IAuthenticationService
    {
        Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password);
        Task RegisterAsync(RegisterDto registerDto);
        Task<UserEntity> GetUserByIdAsync(string userId);
    }
}