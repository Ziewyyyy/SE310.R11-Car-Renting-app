using CarRentalAPI.Models;
using CarRentalAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CarRentalAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        return ToActionResult(await _authService.RegisterAsync(request));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        return ToActionResult(await _authService.LoginAsync(request));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        return ToActionResult(await _authService.RefreshAsync(request));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var accessTokenJti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var expiresValue = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        DateTime? accessTokenExpiresAt = null;
        if (long.TryParse(expiresValue, out var expiresUnix))
        {
            accessTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresUnix).UtcDateTime;
        }

        return ToActionResult(await _authService.LogoutAsync(
            userId,
            request,
            accessTokenJti,
            accessTokenExpiresAt));
    }

    private IActionResult ToActionResult(AuthResult result)
    {
        return result.Response is not null
            ? StatusCode(result.StatusCode, result.Response)
            : StatusCode(result.StatusCode, new { message = result.Message });
    }
}
