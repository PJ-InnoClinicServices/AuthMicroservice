using AuthMicroservice.DataAccess.Entites;

namespace AuthMicroservice.BusinessLogic.Interfaces.IServices;

public interface ITokenService
{
    Task<string> CreateAccessToken(UserEntity userEntity);
    Task<string> CreateRefreshToken();
    Task<string> HashToken(string token);
    Task<bool> ValidateAccessToken(string token);
    Task<(string AccessToken, string newRefreshToken)> RefreshAccessTokenAsync(string refreshToken);

}