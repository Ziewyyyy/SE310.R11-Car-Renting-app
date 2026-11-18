using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using CarRentalAPI.Models;
using CarRentalAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace CarRentalAPI.Services;

public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserProfileService(
        IUserProfileRepository repository,
        IPasswordHasher<User> passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserProfileResult> GetProfileAsync(Guid userId)
    {
        var user = await _repository.FindByIdAsync(userId);
        if (user is null)
        {
            return new UserProfileResult((int)HttpStatusCode.NotFound, "User not found.");
        }

        return new UserProfileResult((int)HttpStatusCode.OK, Profile: ToProfile(user));
    }

    public async Task<UserProfileResult> UpdateProfileAsync(
        Guid userId,
        UpdateUserProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return new UserProfileResult(
                (int)HttpStatusCode.BadRequest,
                "FullName and Email are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return new UserProfileResult(
                (int)HttpStatusCode.BadRequest,
                "Email format is invalid.");
        }

        var user = await _repository.FindByIdAsync(userId);
        if (user is null)
        {
            return new UserProfileResult((int)HttpStatusCode.NotFound, "User not found.");
        }

        if (await _repository.EmailExistsForAnotherUserAsync(email, userId))
        {
            return new UserProfileResult(
                (int)HttpStatusCode.Conflict,
                "Email is already registered.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.Phone = request.Phone?.Trim() ?? "";
        await _repository.SaveChangesAsync();

        return new UserProfileResult((int)HttpStatusCode.OK, Profile: ToProfile(user));
    }

    public async Task<UserProfileResult> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return new UserProfileResult(
                (int)HttpStatusCode.BadRequest,
                "CurrentPassword and NewPassword are required.");
        }

        if (request.NewPassword.Length < 8)
        {
            return new UserProfileResult(
                (int)HttpStatusCode.BadRequest,
                "NewPassword must be at least 8 characters.");
        }

        var user = await _repository.FindByIdAsync(userId);
        if (user is null)
        {
            return new UserProfileResult((int)HttpStatusCode.NotFound, "User not found.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return new UserProfileResult(
                (int)HttpStatusCode.BadRequest,
                "Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _repository.SaveChangesAsync();

        return new UserProfileResult(
            (int)HttpStatusCode.OK,
            "Password changed successfully.");
    }

    public async Task<UserProfileResult> GetAllAsync(string? role, string? keyword)
    {
        var users = await _repository.FindAllAsync(role, keyword);
        return new UserProfileResult(
            (int)HttpStatusCode.OK,
            Profiles: users.Select(ToProfile).ToList());
    }

    private static UserProfileResponse ToProfile(User user)
    {
        return new UserProfileResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.Role);
    }
}
