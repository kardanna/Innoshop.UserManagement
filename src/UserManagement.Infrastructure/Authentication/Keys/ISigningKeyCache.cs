using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Authentication.Keys;

public interface ISigningKeyCache
{
    SemaphoreSlim KeyGenerationSemaphore { get; }
    
    SigningKey? GetKeyPair(Guid id); //Remove
    IEnumerable<RsaSecurityKey> GetUnexpiredValidationKeys();
    RsaSecurityKey? GetKeyForSigning();
    bool TryAddKey(SigningKey key);
    bool TryRemoveKey(Guid id);
    bool IsEmpty { get; }
}