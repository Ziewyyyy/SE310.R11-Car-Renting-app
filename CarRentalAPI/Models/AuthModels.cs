namespace CarRentalAPI.Models;

public sealed record RegisterRequest(string FullName, string Email, string Password, string? Phone);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, UserResponse User);
public sealed record UserResponse(Guid Id, string FullName, string Email, string Phone, string Role);
public sealed record AuthResult(int StatusCode, string? Message = null, AuthResponse? Response = null);
