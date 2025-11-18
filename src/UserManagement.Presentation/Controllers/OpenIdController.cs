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
        var configuration = new
        {
            issuer = _jwtOptions.Issuer,
            jwks_uri = $"{_jwtOptions.Issuer}/.well-known/jwks.json"
        };

        return new JsonResult(configuration);
    }

    [HttpGet("jwks.json")]
    public async Task<IActionResult> GetJwks()
    {
        return new JsonResult(await _signingKeysProvider.GetSigningKeyAsync());
    }
}
