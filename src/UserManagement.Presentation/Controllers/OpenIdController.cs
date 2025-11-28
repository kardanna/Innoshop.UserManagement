using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserManagement.Application.Options;
using UserManagement.Application.UseCases.ValidationKeys;

namespace UserManagement.Presentation.Controllers;

[ApiController]
[Route(".well-known")]
public class OpenIdController : BaseApiController
{
    private readonly JwtOptions _jwtOptions;

    public OpenIdController(
        ISender sender,
        IOptions<JwtOptions> jwtOptions)
        : base(sender)
    {
        _jwtOptions = jwtOptions.Value;
    }

    [HttpGet("openid-configuration")]
    public IActionResult GetConfiguration()
    {
        var issuer = _jwtOptions.Issuer;

        var configuration = new
        {
            issuer = issuer,
            jwks_uri = $"{issuer}/.well-known/jwks.json",
            authorization_endpoint = $"{issuer}/noop",
            token_endpoint = $"{issuer}/noop",
            response_types_supported = new[] { "none" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" }
        };

        return new JsonResult(configuration);
    }

    [HttpGet("jwks.json")]
    public async Task<IActionResult> GetJwks()
    {
        var query = new GetValidationKeysQuery();

        var response = await _sender.Send(query);

        if (response.IsFailure) return HandleFailure(response);

        return new JsonResult( new { keys = response.Value } );
    }
}
