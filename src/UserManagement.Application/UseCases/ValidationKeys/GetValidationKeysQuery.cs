using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.ValidationKeys;

public record GetValidationKeysQuery() : IQuery<IEnumerable<JsonWebKey>>;