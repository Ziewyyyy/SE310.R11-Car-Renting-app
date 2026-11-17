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
