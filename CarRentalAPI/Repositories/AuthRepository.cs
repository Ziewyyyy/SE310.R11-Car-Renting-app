using CarRentalAPI.Data;
using CarRentalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _db;

    public AuthRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return _db.Users.AnyAsync(user => user.Email == email);
    }

    public Task<User?> FindUserByEmailAsync(string email)
    {
        return _db.Users.SingleOrDefaultAsync(user => user.Email == email);
    }

    public Task<User?> FindUserByIdAsync(Guid id)
    {
        return _db.Users.SingleOrDefaultAsync(user => user.Id == id);
    }

    public Task<RefreshToken?> FindRefreshTokenAsync(string tokenHash)
    {
        return _db.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string tokenHash, Guid userId)
    {
        var token = await _db.RefreshTokens
            .SingleOrDefaultAsync(item => item.TokenHash == tokenHash && item.UserId == userId);
        if (token is null)
        {
            return false;
        }

        if (!token.RevokedAt.HasValue)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public Task AddUserAsync(User user)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        _db.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
