using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Authentication.Keys;

public class ValidationKeysProvider : IValidationKeysProvider
{
    private readonly ISigningKeyCache _cache;

    public ValidationKeysProvider(ISigningKeyCache cache)
    {
        _cache = cache;
    }

    public IEnumerable<JsonWebKey> GetJsonWebKeys()
    {
        return _cache
            .GetUnexpiredValidationKeys()
            .Select(k => JsonWebKeyConverter.ConvertFromRSASecurityKey(k));
    }
}