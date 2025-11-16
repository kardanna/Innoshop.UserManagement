using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Authentication.Keys;

public interface ISigningKeyCache
{
    SemaphoreSlim KeyGenerationSemaphore { get; }
    
    IEnumerable<RsaSecurityKey> GetUnexpiredValidationKeys();
    RsaSecurityKey? GetKeyForSigning();
    bool TryAddKey(SigningKey key);
    void RemoveExpiredKeys();
    bool IsEmpty { get; }
}