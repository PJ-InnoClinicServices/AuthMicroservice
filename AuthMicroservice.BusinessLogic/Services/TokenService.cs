using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthMicroservice.BusinessLogic.Interfaces.IServices;
using AuthMicroservice.DataAccess.Entites;
using AuthMicroservice.DataAccess.IRepositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
namespace AuthMicroservice.BusinessLogic.Services;

    public class TokenService(IConfiguration config, ITokenRepository tokenRepository) : ITokenService
    {
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

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token)); 
        }

        public async Task<string> CreateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return await Task.FromResult(Convert.ToBase64String(randomBytes)); // Task.FromResult
        }

        public async Task<string> HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return await Task.FromResult(Convert.ToBase64String(hashedBytes)); // Task.FromResult
        }

        public async Task<bool> ValidateAccessToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SigningKey"]));
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = config["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return await Task.FromResult(principal != null); 
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<(string AccessToken, string newRefreshToken)> RefreshAccessTokenAsync(string refreshToken)
        {
            var hashedToken = await HashToken(refreshToken);
            
            var user = await tokenRepository.FindByRefreshTokenAsync(hashedToken);
            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }
            
            var newAccessToken = await CreateAccessToken(user);
            var newRefreshToken = await CreateRefreshToken();
            var newRefreshTokenHash = await HashToken(newRefreshToken);
            
            user.RefreshTokenHash = newRefreshTokenHash;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await tokenRepository.UpdateAsync(user);

            return (newAccessToken, newRefreshToken);
        }
    }

