using CarRentalAPI.Models;

namespace CarRentalAPI.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshAsync(RefreshRequest request);
}
