using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Application.Interfaces;

public interface IValidationKeysProvider
{
    IEnumerable<JsonWebKey> GetJsonWebKeys();
}