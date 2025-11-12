using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Repositories;

public class EmailVerificationAttemptRepository : IEmailVerificationAttemptRepository
{
    private readonly ApplicationContext _appContext;

    public EmailVerificationAttemptRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public void Add(EmailVerificationAttempt attempt)
    {
        _appContext.EmailVerificationAttempts.Add(attempt);
    }

    public async Task<EmailVerificationAttempt?> GetAsync(string attemptCode)
    {
        return await _appContext.EmailVerificationAttempts
            .Include(a => a.User)
            .Where(a => a.VerificationCode == attemptCode)
            .FirstOrDefaultAsync();
    }
}