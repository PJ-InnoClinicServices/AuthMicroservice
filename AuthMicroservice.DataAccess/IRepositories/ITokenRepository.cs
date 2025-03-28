using AuthMicroservice.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;

namespace AuthMicroservice.DataAccess.IRepositories;

public interface ITokenRepository
{
    Task<UserEntity> FindByRefreshTokenAsync(string refreshTokenHash);
    Task<IdentityResult> UpdateAsync(UserEntity user);
    Task<string> CreateAccessToken(UserEntity userEntity);
    Task<string> CreateRefreshToken();
    Task<string> HashToken(string token);
}