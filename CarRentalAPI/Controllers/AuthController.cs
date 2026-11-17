using CarRentalAPI.Data;
using CarRentalAPI.Models;
using CarRentalAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CarRentalAPI.Controllers;

public sealed record RegisterRequest(string FullName, string Email, string Password, string? Phone);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, UserResponse User);
public sealed record UserResponse(Guid Id, string FullName, string Email, string Phone, string Role);

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtTokenService _tokenService;

    public AuthController(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        JwtTokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "FullName, Email and Password are required." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (!IsValidEmail(email))
        {
            return BadRequest(new { message = "Email format is invalid." });
        }

        if (request.Password.Length < 8)
        {
            return BadRequest(new { message = "Password must be at least 8 characters." });
        }

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "Email is already registered." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone?.Trim() ?? "",
            Role = "Customer"
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(await CreateAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (!IsValidEmail(email))
        {
            return BadRequest(new { message = "Email format is invalid." });
        }

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null ||
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(await CreateAuthResponse(user));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "RefreshToken is required." });
        }

        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _db.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (storedToken is null ||
            storedToken.RevokedAt.HasValue ||
            storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == storedToken.UserId);
        if (user is null)
        {
            return Unauthorized(new { message = "User no longer exists." });
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        var response = await CreateAuthResponse(user);
        await _db.SaveChangesAsync();
        return Ok(response);
    }

    private static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private async Task<AuthResponse> CreateAuthResponse(User user)
    {
        var refreshToken = _tokenService.CreateRefreshToken();
        var accessTokenExpiresAt = _tokenService.GetAccessTokenExpiry();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = _tokenService.GetRefreshTokenExpiry()
        });
        await _db.SaveChangesAsync();
        return new AuthResponse(
            _tokenService.CreateAccessToken(user),
            refreshToken,
            accessTokenExpiresAt,
            new UserResponse(user.Id, user.FullName, user.Email, user.Phone, user.Role));
    }
}
