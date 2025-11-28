using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.ValidationKeys;

public class GetValidationKeysQueryHandler : IQueryHandler<GetValidationKeysQuery, IEnumerable<JsonWebKey>>
{
    private readonly IValidationKeysProvider _provider;

    public GetValidationKeysQueryHandler(IValidationKeysProvider provider)
    {
        _provider = provider;
    }

    public async Task<Result<IEnumerable<JsonWebKey>>> Handle(GetValidationKeysQuery request, CancellationToken cancellationToken)
    {
        return Result.Success(_provider.GetJsonWebKeys());
    }
}