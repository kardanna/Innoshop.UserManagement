namespace UserManagement.Domain.Entities;

public class PasswordRestoreAttempt
{
    public Guid Id { get; set; }
    public string AttemptCode { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime AttemptedAt { get; set; }
    public bool IsSucceeded { get; set; }
    public DateTime? SucceededAt { get; set; }
}