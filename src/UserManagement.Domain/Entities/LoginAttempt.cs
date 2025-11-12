namespace UserManagement.Domain.Entities;

public class LoginAttempt
{
    public string Email { get; set; } = null!;
    public DateTime AttemtedAt { get; set; }
    public string? DeviceFingerprint { get; set; }
}