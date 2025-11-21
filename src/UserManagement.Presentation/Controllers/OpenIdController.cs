using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserManagement.Infrastructure.Authentication.Configuration;
using UserManagement.Infrastructure.Authentication.Keys;

namespace UserManagement.Presentation.Controllers;

[ApiController]
[Route(".well-known")]
public class OpenIdController : ControllerBase
{
    private readonly ISigningKeyProvider _signingKeysProvider;
    private readonly JwtOptions _jwtOptions;

    public OpenIdController(
        ISigningKeyProvider signingKeysProvider,
        IOptions<JwtOptions> jwtOptions)
    {
        _signingKeysProvider = signingKeysProvider;
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
        var keys = _signingKeysProvider.GetJsonWebKeys();
        return new JsonResult( new { keys } );
    }
}
