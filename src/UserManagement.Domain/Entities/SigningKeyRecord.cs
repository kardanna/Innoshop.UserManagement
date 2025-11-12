namespace UserManagement.Domain.Entities;

public class SigningKeyRecord
{
    public Guid Id { get; set; }
    public string PublicKeyPem { get; set; } = null!;
    public string PrivateKeyPem { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
}