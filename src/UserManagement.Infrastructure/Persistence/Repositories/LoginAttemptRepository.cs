using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Repositories;

public class LoginAttemptRepository : ILoginAttemptRepository
{
    private readonly ApplicationContext _appContext;

    public LoginAttemptRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task<int> CountLoginAttemptsAsync(string email, int timeWindowInMinutes)
    {
        return await _appContext.LoginAttempts
            .Where(a => a.Email == email && a.AttemtedAt > DateTime.UtcNow.AddMinutes(-timeWindowInMinutes))
            .CountAsync();
    }

    public void AddAttempt(string email, string? deviceFingerprint = null)
    {
        var attempt = new LoginAttempt()
        {
            Email = email,
            AttemtedAt = DateTime.UtcNow,
            DeviceFingerprint = deviceFingerprint            
        };

        _appContext.LoginAttempts.Add(attempt);
    }
}