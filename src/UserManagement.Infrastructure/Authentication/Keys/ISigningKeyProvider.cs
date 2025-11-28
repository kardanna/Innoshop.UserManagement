using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Authentication.Keys;

public interface ISigningKeyProvider
{
    Task<RsaSecurityKey> GetSigningKeyAsync();
}