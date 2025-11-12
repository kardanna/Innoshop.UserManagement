using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserManagement.Infrastructure.Authentication.Configuration;
using UserManagement.Infrastructure.Authentication.Keys;

namespace UserManagement.API.Controllers;

[ApiController]
[Route(".well-known")]
public class OpenIdController : ControllerBase
{
    private readonly ILogger<OpenIdController> _logger;
    private readonly ISigningKeyProvider _signingKeysProvider;
    private readonly JwtOptions _jwtOptions;

    public OpenIdController(
        ILogger<OpenIdController> logger,
        ISigningKeyProvider signingKeysProvider,
        IOptions<JwtOptions> jwtOptions)
    {
        _logger = logger;
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
