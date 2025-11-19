using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Repositories;

public class UserDeactivationRepository : IUserDeactivationRepository
{
    private readonly ApplicationContext _appContext;

    public UserDeactivationRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public void Add(UserDeactivation record)
    {
        _appContext.UserDeactivations.Add(record);
    }

    public async Task<UserDeactivation?> GetLatestAsync(Guid userId)
    {
        return await _appContext.UserDeactivations
            .Include(r => r.User)
                .ThenInclude(u => u.Roles)
            .Include(r => r.DeactivationRequester)
                .ThenInclude(u => u.Roles)
            .Include(r => r.ReactivationRequester)
                .ThenInclude(u => u!.Roles)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.DeactivatedAt)
            .FirstOrDefaultAsync();
    }

    public void RemoveAllForUser(Guid userId)
    {
        var records = _appContext.UserDeactivations
            .Where(r => r.UserId == userId);
        
        _appContext.UserDeactivations.RemoveRange(records);
    }
}