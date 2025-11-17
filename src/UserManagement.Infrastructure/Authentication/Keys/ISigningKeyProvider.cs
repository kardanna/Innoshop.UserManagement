using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Authentication.Keys;

public interface ISigningKeyProvider
{
    IEnumerable<JsonWebKey> GetJsonWebKeys();
    Task<RsaSecurityKey> GetSigningKeyAsync();
}