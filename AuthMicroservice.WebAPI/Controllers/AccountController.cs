using System.Security.Claims;
using AuthMicroservice.BusinessLogic.Interfaces;
using AuthMicroservice.BusinessLogic.Interfaces.IServices;
using AuthMicroservice.Shared.Dtos.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthenticationService = AuthMicroservice.BusinessLogic.Interfaces.IServices.IAuthenticationService;

namespace AuthMicroservice.Controllers;

[Route("")]
[ApiController]
public class AccountController(IAuthenticationService authenticationService, ITokenService tokenService)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid request data.", errors = ModelState });

        var (accessToken, refreshToken) = await authenticationService.LoginAsync(loginDto.Email, loginDto.Password);
        if (accessToken == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new { AccessToken = accessToken });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid registration data.", errors = ModelState });

        try
        {
            await authenticationService.RegisterAsync(registerDto);
            return Ok(new { message = "User registered successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            return BadRequest("Refresh token is required");
        
        var (accessToken, newRefreshToken) = await tokenService.RefreshAccessTokenAsync(refreshToken);
        
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(new { AccessToken = accessToken });
    }


    [HttpGet("users/me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var claims = User.Claims.ToList();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userId == null)
            return Unauthorized(new { message = "No ID in the token." });

        var user = await authenticationService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(new
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email
        });
    }
}
