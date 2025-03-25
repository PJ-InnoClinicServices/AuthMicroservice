using Microsoft.AspNetCore.Identity;

namespace AuthMicroservice.DataAccess.Entites;

public class UserEntity : IdentityUser
{
    public string RefreshTokenHash { get; set; } 
    public DateTime RefreshTokenExpiry { get; set; } 
}