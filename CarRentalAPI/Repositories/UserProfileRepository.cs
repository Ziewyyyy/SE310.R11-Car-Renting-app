using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _db;

    public UserProfileRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> FindByIdAsync(Guid id)
    {
        return _db.Users.SingleOrDefaultAsync(user => user.Id == id);
    }

    public Task<bool> EmailExistsForAnotherUserAsync(string email, Guid userId)
    {
        return _db.Users.AnyAsync(user => user.Email == email && user.Id != userId);
    }

    public async Task<IReadOnlyList<User>> FindAllAsync(string? role, string? keyword)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(user => user.Role == role);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim().ToLower();
            query = query.Where(user =>
                user.FullName.ToLower().Contains(normalizedKeyword) ||
                user.Email.ToLower().Contains(normalizedKeyword));
        }

        return await query
            .OrderBy(user => user.FullName)
            .ToListAsync();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
