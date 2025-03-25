using AuthMicroservice.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;

namespace AuthMicroservice.DataAccess.IRepositories
{
    public interface IUserRepository
    {
        Task<UserEntity> FindByEmailAsync(string email);
        Task<IdentityResult> CreateAsync(UserEntity user, string password);
        Task<IdentityResult> UpdateAsync(UserEntity user);
        Task<SignInResult> CheckPasswordSignInAsync(UserEntity user, string password, bool lockoutOnFailure);
        Task<SignInResult> PasswordSignInAsync(UserEntity user, string password, bool isPersistent, bool lockoutOnFailure);
        Task<UserEntity> FindByIdAsync(string userId); 
    }
}