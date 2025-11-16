namespace UserManagement.Domain.Entities;

public class TokenRecord
{
    public Guid AccessTokenId { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public bool IsAccessTokenActive => AccessTokenExpiresAt > DateTime.UtcNow;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string DeviceFingerprint { get; set; } = null!;

    public TokenRecord() { }

    public TokenRecord(
        Guid accessTokenId,
        TimeSpan accessTokenLifetime,
        Guid userId,
        string refreshToken,
        string deviceFingerprint)
    {
        AccessTokenId = accessTokenId;
        IssuedAt = DateTime.UtcNow;
        AccessTokenExpiresAt = IssuedAt.Add(accessTokenLifetime);
        UserId = userId;
        RefreshToken = refreshToken;
        DeviceFingerprint = deviceFingerprint;
    }
}