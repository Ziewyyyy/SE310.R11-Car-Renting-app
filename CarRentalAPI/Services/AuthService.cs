using System.Net;
using System.Text.RegularExpressions;
using CarRentalAPI.Models;
using CarRentalAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace CarRentalAPI.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _repository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtTokenService _tokenService;
    private readonly IJwtBlacklistService _jwtBlacklist;

    public AuthService(
        IAuthRepository repository,
        IPasswordHasher<User> passwordHasher,
        JwtTokenService tokenService,
        IJwtBlacklistService jwtBlacklist)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtBlacklist = jwtBlacklist;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResult((int)HttpStatusCode.BadRequest, "FullName, Email and Password are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (!IsValidEmail(email))
        {
            return new AuthResult((int)HttpStatusCode.BadRequest, "Email format is invalid.");
        }

        if (request.Password.Length < 8)
        {
            return new AuthResult((int)HttpStatusCode.BadRequest, "Password must be at least 8 characters.");
        }

        if (await _repository.EmailExistsAsync(email))
        {
            return new AuthResult((int)HttpStatusCode.Conflict, "Email is already registered.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone?.Trim() ?? "",
            Role = "Customer"
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _repository.AddUserAsync(user);
        await _repository.SaveChangesAsync();

        return new AuthResult((int)HttpStatusCode.OK, Response: await CreateAuthResponseAsync(user));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResult((int)HttpStatusCode.BadRequest, "Email and Password are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (!IsValidEmail(email))
        {
            return new AuthResult((int)HttpStatusCode.BadRequest, "Email format is invalid.");
        }

        var user = await _repository.FindUserByEmailAsync(email);
        if (user is null ||
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) ==
            PasswordVerificationResult.Failed)
        {
            return new AuthResult((int)HttpStatusCode.Unauthorized, "Invalid email or password.");
        }

        return new AuthResult((int)HttpStatusCode.OK, Response: await CreateAuthResponseAsync(user));
    }

    public async Task<AuthResult> RefreshAsync(RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new AuthResult((int)HttpStatusCode.BadRequest, "RefreshToken is required.");
        }

        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _repository.FindRefreshTokenAsync(tokenHash);
        if (storedToken is null ||
            storedToken.RevokedAt.HasValue ||
            storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return new AuthResult((int)HttpStatusCode.Unauthorized, "Invalid or expired refresh token.");
        }

        var user = await _repository.FindUserByIdAsync(storedToken.UserId);
        if (user is null)
        {
            return new AuthResult((int)HttpStatusCode.Unauthorized, "User no longer exists.");
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        return new AuthResult((int)HttpStatusCode.OK, Response: await CreateAuthResponseAsync(user));
    }

    public async Task<AuthResult> LogoutAsync(
        Guid userId,
        LogoutRequest request,
        string? accessTokenJti,
        DateTime? accessTokenExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new AuthResult((int)HttpStatusCode.BadRequest, "RefreshToken is required.");
        }

        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var revoked = await _repository.RevokeRefreshTokenAsync(tokenHash, userId);
        if (!revoked)
        {
            return new AuthResult((int)HttpStatusCode.BadRequest, "Refresh token is invalid.");
        }

        if (!string.IsNullOrWhiteSpace(accessTokenJti))
        {
            _jwtBlacklist.Add(accessTokenJti, accessTokenExpiresAt ?? DateTime.UtcNow);
        }

        return new AuthResult((int)HttpStatusCode.OK, "Logged out successfully.");
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user)
    {
        var refreshToken = _tokenService.CreateRefreshToken();
        var accessTokenExpiresAt = _tokenService.GetAccessTokenExpiry();
        await _repository.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = _tokenService.GetRefreshTokenExpiry()
        });
        await _repository.SaveChangesAsync();

        return new AuthResponse(
            _tokenService.CreateAccessToken(user),
            refreshToken,
            accessTokenExpiresAt,
            new UserResponse(user.Id, user.FullName, user.Email, user.Phone, user.Role));
    }

    private static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
