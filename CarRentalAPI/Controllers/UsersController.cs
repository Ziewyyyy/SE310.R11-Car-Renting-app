using System.Security.Claims;
using CarRentalAPI.Models;
using CarRentalAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalAPI.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _profileService;

    public UsersController(IUserProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return ToActionResult(await _profileService.GetProfileAsync(userId.Value));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UpdateUserProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return ToActionResult(await _profileService.UpdateProfileAsync(userId.Value, request));
    }

    [HttpPatch("me/password")]
    public async Task<IActionResult> ChangeMyPassword(ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return ToActionResult(await _profileService.ChangePasswordAsync(userId.Value, request));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? role,
        [FromQuery] string? keyword)
    {
        return ToActionResult(await _profileService.GetAllAsync(role, keyword));
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private IActionResult ToActionResult(UserProfileResult result)
    {
        if (result.Profile is not null)
        {
            return StatusCode(result.StatusCode, result.Profile);
        }

        if (result.Profiles is not null)
        {
            return StatusCode(result.StatusCode, result.Profiles);
        }

        return StatusCode(
            result.StatusCode,
            new { message = result.Message });
    }
}
