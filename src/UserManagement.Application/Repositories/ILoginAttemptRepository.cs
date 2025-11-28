namespace UserManagement.Application.Repositories;

public interface ILoginAttemptRepository
{
    Task<int> CountLoginAttemptsAsync(string email, int timeWindowInMinutes);
    void AddAttempt(string email, string? deviceFingerprint);
    Task<int> RemoveOldLoginAttemptsAsync(int attepmtConsideredOldAfterMinutes);
}