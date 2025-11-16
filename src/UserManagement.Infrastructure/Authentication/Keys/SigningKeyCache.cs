using System.Collections.Concurrent;
using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Authentication.Keys;

public class SigningKeysCache : ISigningKeyCache
{
    private readonly ConcurrentDictionary<Guid, SigningKey> _cache = new();
    public SemaphoreSlim KeyGenerationSemaphore { get; } = new(1, 1);

    public IEnumerable<RsaSecurityKey> GetUnexpiredValidationKeys()
    {
        return _cache
            .Where(sk => sk.Value.ExpiresAt > DateTime.UtcNow)
            .Select(sk => sk.Value.PublicKey);
    }
    
    public RsaSecurityKey? GetKeyForSigning()
    {
        return _cache
            .Where(sk => sk.Value.SigningExpiresAt > DateTime.UtcNow)
            .OrderByDescending(sk => sk.Value.SigningExpiresAt)
            .FirstOrDefault()
            .Value
            ?.PrivateKey;
    }

    public bool TryAddKey(SigningKey key)
    {
        return _cache.TryAdd(key.Id, key);
    }

    public void RemoveExpiredKeys()
    {
        var expiredKeysIds = _cache.Values
            .Where(k => k.ExpiresAt < DateTime.UtcNow)
            .Select(k => k.Id);

        foreach (var keyId in expiredKeysIds)
        {
            _cache.Remove(keyId, out var key);
        }
    }

    public bool IsEmpty => _cache.IsEmpty;
}