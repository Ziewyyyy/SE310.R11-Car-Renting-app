using CarRentalAPI.Models;

namespace CarRentalAPI.Repositories;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> FindUserByEmailAsync(string email);
    Task<User?> FindUserByIdAsync(Guid id);
    Task<RefreshToken?> FindRefreshTokenAsync(string tokenHash);
    Task<bool> RevokeRefreshTokenAsync(string tokenHash, Guid userId);
    Task AddUserAsync(User user);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
}
