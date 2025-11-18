using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Repositories;

public class EmailVerificationAttemptRepository : IEmailVerificationAttemptRepository
{
    private readonly ApplicationContext _appContext;

    public EmailVerificationAttemptRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public void Add(EmailVerificationAttempt attempt)
    {
        _appContext.EmailVerificationAttempts
            .Add(attempt);
    }

    public async Task<EmailVerificationAttempt?> GetAsync(string attemptCode)
    {
        return await _appContext.EmailVerificationAttempts
            .Include(a => a.User)
            .Where(a => a.VerificationCode == attemptCode)
            .FirstOrDefaultAsync();
    }

    public void RemoveUnseccessfulAttemptsFor(string email)
    {
        var attempts = _appContext.EmailVerificationAttempts
            .Where(a => a.PreviousEmail != null && a.PreviousEmail == email && a.IsSucceeded == false);

        _appContext.EmailVerificationAttempts
            .RemoveRange(attempts);
    }

    public Task<EmailVerificationAttempt?> GetLastSuccessfulAttemptAsync(string email)
    {
        return _appContext.EmailVerificationAttempts
            .Where(a => a.Email == email && a.IsSucceeded)
            .OrderByDescending(a => a.AttemptedAt)
            .FirstOrDefaultAsync();
    }

    public void RemoveAllUserAttempts(Guid userId)
    {
        var attempts = _appContext.EmailVerificationAttempts
            .Where(a => a.UserId == userId);
        
        _appContext.EmailVerificationAttempts
            .RemoveRange(attempts);
    }
}