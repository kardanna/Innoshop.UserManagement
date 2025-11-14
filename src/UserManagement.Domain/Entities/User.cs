namespace UserManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; } = null!;
    public bool IsEmailVerified { get; set; }
    public string PasswordHash { get; set; } = null!;
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public bool IsDeactivated { get; set; }
    public DateTime? DeactivationRequestedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletionRequestedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}