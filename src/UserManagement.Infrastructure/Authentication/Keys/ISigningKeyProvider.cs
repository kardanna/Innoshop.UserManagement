using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Authentication.Keys;

public interface ISigningKeyProvider
{
    Task InitializeSigningKeysCacheAsync();
    IEnumerable<JsonWebKey> GetJsonWebKeys();
    Task<RsaSecurityKey> GetSigningKeyAsync();
}