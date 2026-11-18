using CarRentalAPI.Models;

namespace CarRentalAPI.Repositories;

public interface IUserProfileRepository
{
    Task<User?> FindByIdAsync(Guid id);
    Task<bool> EmailExistsForAnotherUserAsync(string email, Guid userId);
    Task<IReadOnlyList<User>> FindAllAsync(string? role, string? keyword);
    Task SaveChangesAsync();
}
