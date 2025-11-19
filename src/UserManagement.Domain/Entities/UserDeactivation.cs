namespace UserManagement.Domain.Entities;

public class UserDeactivation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime DeactivatedAt { get; set; }    
    public Guid DeactivationRequesterId { get; set; }
    public User DeactivationRequester { get; set; } = null!;

    public string Commentary { get; set; } = null!;

    public bool IsActive => ReactivatedAt is not null && ReactivatedAt < DateTime.UtcNow;

    public DateTime? ReactivatedAt { get; set; }
    public Guid? ReactivationRequesterId { get; set; }
    public User? ReactivationRequester { get; set; }
}