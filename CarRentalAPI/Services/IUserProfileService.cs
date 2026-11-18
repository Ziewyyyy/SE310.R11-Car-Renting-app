using CarRentalAPI.Models;

namespace CarRentalAPI.Services;

public interface IUserProfileService
{
    Task<UserProfileResult> GetProfileAsync(Guid userId);
    Task<UserProfileResult> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request);
    Task<UserProfileResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<UserProfileResult> GetAllAsync(string? role, string? keyword);
}
