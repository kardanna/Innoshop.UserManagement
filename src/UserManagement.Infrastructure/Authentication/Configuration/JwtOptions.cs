namespace UserManagement.Infrastructure.Authentication.Configuration;

public class JwtOptions
{
    public string Audience { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public int AccessTokenLifetimeMinutes { get; init; }
    public int RefreshTokenLifetimeMinutes { get; init; }
}