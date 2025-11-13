using UserManagement.Domain.Entities;

namespace UserManagement.Application.Repositories;

public interface IEmailVerificationAttemptRepository
{
    void Add(EmailVerificationAttempt attempt);
    Task<EmailVerificationAttempt?> GetAsync(string attemptCode);
    void RemoveUnseccessfulAttemptsFor(string email);
    Task<EmailVerificationAttempt?> GetLastSuccessfulAttemptAsync(string email);
}