using AuthMicroservice.DataAccess.Entites;

namespace AuthMicroservice.BusinessLogic.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(UserEntity userEntity);
    string CreateRefreshToken();
    string HashToken(string token);
    bool ValidateAccessToken(string token);
}