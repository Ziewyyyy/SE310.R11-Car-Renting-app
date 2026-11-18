using CarRentalAPI.Models;

namespace CarRentalAPI.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshAsync(RefreshRequest request);
    Task<AuthResult> LogoutAsync(Guid userId, LogoutRequest request, string? accessTokenJti, DateTime? accessTokenExpiresAt);
}
