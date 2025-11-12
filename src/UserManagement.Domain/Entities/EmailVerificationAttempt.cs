namespace UserManagement.Domain.Entities;

public class EmailVerificationAttempt
{
    public Guid Id { get; set; }
    public string VerificationCode { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PreviousEmail { get; set; }
    public DateTime AttemptedAt { get; set; }
    public bool IsSucceeded { get; set; }
    public DateTime? SucceededAt { get; set; }
}