using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Interfaces;
using UserManagement.Infrastructure.Authentication.Keys;
using UserManagement.Infrastructure.Authentication.Tokens;

namespace UserManagement.Infrastructure.Authentication;

public static class AuthenticationDependencyInjection
{
    public static void AddAuth(this IServiceCollection services)
    {
        services.AddSingleton<ISigningKeyCache, SigningKeysCache>();

        services.AddScoped<ISigningKeyProvider, SigningKeyProvider>();
        services.AddScoped<IValidationKeysProvider, ValidationKeysProvider>();
        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddHostedService<SigningKeyCacheInitializer>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);
        
        services.AddDataProtection()
            .SetApplicationName("Innoshop.UserManagement");
    }
}