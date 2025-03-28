using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthMicroservice.DataAccess.Entites;
using AuthMicroservice.DataAccess.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthMicroservice.DataAccess.Repositories;

public class TokenRepository(UserManager<UserEntity> userManager, IConfiguration config) : ITokenRepository
{
    public async Task<UserEntity> FindByRefreshTokenAsync(string refreshTokenHash)
    {
        return await userManager.Users.FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash);
    }

    public async Task<IdentityResult> UpdateAsync(UserEntity user)
    {
        return await userManager.UpdateAsync(user);
    }

    public async Task<string> CreateAccessToken(UserEntity userEntity)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userEntity.Id),
            new Claim(JwtRegisteredClaimNames.Email, userEntity.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, userEntity.UserName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SigningKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1), 
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> CreateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes); 
    }

    public async Task<string> HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }
}