using CarRentalAPI.Models;
using CarRentalAPI.Services;
using Microsoft.AspNetCore.Mvc;

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

    private IActionResult ToActionResult(AuthResult result)
    {
        return result.Response is not null
            ? StatusCode(result.StatusCode, result.Response)
            : StatusCode(result.StatusCode, new { message = result.Message });
    }
}
