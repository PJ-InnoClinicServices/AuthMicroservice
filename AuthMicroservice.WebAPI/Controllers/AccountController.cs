using System.Security.Claims;
using AuthMicroservice.BusinessLogic.Interfaces;
using AuthMicroservice.DataAccess.Entites;
using AuthMicroservice.Shared.Dtos.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Controllers;

[Route("")]
[ApiController]
public class AccountController(
    UserManager<UserEntity> userManager,
    ITokenService tokenService,
    SignInManager<UserEntity> signInManager)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == loginDto.Email.ToLower());
        if (user == null) return Unauthorized("Invalid email!");

        var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded) return Unauthorized("Email not found and/or password incorrect");

        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();
        var refreshTokenHash = tokenService.HashToken(refreshToken);

        user.RefreshTokenHash = refreshTokenHash;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);
        
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new
        {
            UserName = user.UserName,
            Email = user.Email,
            AccessToken = accessToken
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var refreshToken = tokenService.CreateRefreshToken();

        var user = new UserEntity
        {
            UserName = registerDto.Email.Split('@')[0],
            Email = registerDto.Email,
            RefreshTokenHash = tokenService.HashToken(refreshToken),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
        };

        var createdUser = await userManager.CreateAsync(user, registerDto.Password);

        if (!createdUser.Succeeded)
            return StatusCode(400, createdUser.Errors);

        var savedUser = await userManager.FindByEmailAsync(user.Email);
        if (savedUser == null)
            return StatusCode(500, "User not found after creation.");

        var accessToken = tokenService.CreateAccessToken(savedUser);
        if (string.IsNullOrEmpty(accessToken))
            return StatusCode(500, "Failed to generate access token.");

        // Ustawienie refresh tokena w HttpOnly cookie
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new
        {
            Id = savedUser.Id,
            UserName = savedUser.UserName,
            Email = savedUser.Email,
            AccessToken = accessToken
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            return BadRequest("Refresh token is required");

        var hashedToken = tokenService.HashToken(refreshToken);

        var user = await userManager.Users.FirstOrDefaultAsync(x => x.RefreshTokenHash == hashedToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token");

        var newAccessToken = tokenService.CreateAccessToken(user);
        var newRefreshToken = tokenService.CreateRefreshToken();
        var newRefreshTokenHash = tokenService.HashToken(newRefreshToken);

        user.RefreshTokenHash = newRefreshTokenHash;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        // Ustawienie nowego refresh tokena w HttpOnly cookie
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new { AccessToken = newAccessToken });
    }

    [Authorize]
    [HttpGet("users/me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { message = "No ID in the token" });

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "Cant find user!" });

        return Ok(new
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email
        });
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken()
    {
        var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return BadRequest("Access token is missing or invalid format.");
        }

        var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();
        var isValid = tokenService.ValidateAccessToken(accessToken);

        if (!isValid)
        {
            return Unauthorized("Invalid or expired access token.");
        }

        return Ok("Access token is valid.");
    }
}

