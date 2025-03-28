using AuthMicroservice.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthMicroservice.DataAccess.IRepositories;

namespace AuthMicroservice.DataAccess.Repositories
{
    public class UserRepository(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager)
        : IUserRepository
    {
        public async Task<UserEntity> FindByEmailAsync(string email)
        {
            return await userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task<IdentityResult> CreateAsync(UserEntity user, string password)
        {
            return await userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> UpdateAsync(UserEntity user)
        {
            return await userManager.UpdateAsync(user);
        }

        public async Task<SignInResult> CheckPasswordSignInAsync(UserEntity user, string password, bool lockoutOnFailure)
        {
            return await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
        }

        public async Task<SignInResult> PasswordSignInAsync(UserEntity user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return await signInManager.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }
        
        public async Task<UserEntity> FindByIdAsync(string userId) // Nowa metoda
        {
            return await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}