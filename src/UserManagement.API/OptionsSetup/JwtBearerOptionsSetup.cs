using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Infrastructure.Authentication.Configuration;
using UserManagement.Infrastructure.Authentication.Keys;

namespace UserManagement.API.OptionsSetup;

public class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions;
    private readonly ISigningKeyCache _cache;

    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions, ISigningKeyCache cache)
    {
        _jwtOptions = jwtOptions.Value;
        _cache = cache;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme) return;

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
            {
                return _cache.GetUnexpiredValidationKeys();
            }
        };
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(JwtBearerDefaults.AuthenticationScheme, options);
    }
}