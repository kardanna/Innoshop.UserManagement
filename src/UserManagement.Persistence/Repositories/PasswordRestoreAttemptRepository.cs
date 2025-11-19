using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Repositories;

public class PasswordRestoreAttemptRepository : IPasswordRestoreAttemptRepository
{
    private readonly ApplicationContext _appContext;

    public PasswordRestoreAttemptRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public void Add(PasswordRestoreAttempt attempt)
    {
        _appContext.PasswordRestoreAttempts.Add(attempt);
    }

    public void RemovePreviousUnseccessfulAttempts(Guid userId)
    {
        var attempts = _appContext.PasswordRestoreAttempts
            .Where(a => a.UserId == userId && !a.IsSucceeded);
        
        _appContext.PasswordRestoreAttempts.RemoveRange(attempts);
    }

    public async Task<PasswordRestoreAttempt?> GetAsync(string attemptCode)
    {
        return await _appContext.PasswordRestoreAttempts
            .Include(a => a.User)
            .Where(a => a.AttemptCode == attemptCode)
            .FirstOrDefaultAsync();
    }

}