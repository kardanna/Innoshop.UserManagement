using System.Collections.Concurrent;
using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Authentication.Keys;

public class SigningKeysCache : ISigningKeyCache
{
    private readonly ConcurrentDictionary<Guid, SigningKey> _cache = new();
    public SemaphoreSlim KeyGenerationSemaphore { get; } = new(1, 1);

    public SigningKey? GetKeyPair(Guid id) //Remove
    {
        if (_cache.TryGetValue(id, out var keyPair))
        {
            return keyPair;
        }
        return null;
    }

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

    public bool TryRemoveKey(Guid id)
    {
        return _cache.TryRemove(id, out _);
    }

    public bool IsEmpty => _cache.IsEmpty;
}