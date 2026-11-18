namespace CarRentalAPI.Models;

public sealed record UpdateUserProfileRequest(string FullName, string Email, string Phone);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record UserProfileResponse(Guid Id, string FullName, string Email, string Phone, string Role);
public sealed record UserProfileResult(
    int StatusCode,
    string? Message = null,
    UserProfileResponse? Profile = null,
    IReadOnlyList<UserProfileResponse>? Profiles = null);
