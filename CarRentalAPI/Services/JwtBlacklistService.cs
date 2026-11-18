using System.Collections.Concurrent;

namespace CarRentalAPI.Services;

public interface IJwtBlacklistService
{
    void Add(string jti, DateTime expiresAt);
    bool Contains(string jti);
}

public sealed class JwtBlacklistService : IJwtBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _tokens = new();

    public void Add(string jti, DateTime expiresAt)
    {
        if (!string.IsNullOrWhiteSpace(jti))
        {
            _tokens[jti] = expiresAt;
        }
    }

    public bool Contains(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return false;
        }

        if (_tokens.TryGetValue(jti, out var expiresAt) &&
            expiresAt > DateTime.UtcNow)
        {
            return true;
        }

        _tokens.TryRemove(jti, out _);
        return false;
    }
}
