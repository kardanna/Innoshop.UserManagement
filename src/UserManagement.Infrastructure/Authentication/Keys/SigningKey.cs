using Microsoft.IdentityModel.Tokens;

namespace UserManagement.Infrastructure.Authentication.Keys;

public class SigningKey
{
    public Guid Id { get; init; }
    public RsaSecurityKey PrivateKey { get; init; } = null!;
    public RsaSecurityKey PublicKey { get; init; } = null!;
    public DateTime IssuedAt { get; init; }
    public DateTime SigningExpiresAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    
    public SigningKey() { }
};