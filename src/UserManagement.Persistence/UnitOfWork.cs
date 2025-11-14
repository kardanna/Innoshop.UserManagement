using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationContext _appContext;

    public UnitOfWork(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task SaveChangesAsync()
    {
        var createdUsers = _appContext.ChangeTracker
            .Entries<User>()
            .Where(e => e.State == EntityState.Added);
        
        foreach (var user in createdUsers)
        {
            user.Entity.CreatedAt = DateTime.UtcNow;
        }

        var modifiedUsers = _appContext.ChangeTracker
            .Entries<User>()
            .Where(e => e.State == EntityState.Modified);
        
        foreach (var user in modifiedUsers)
        {
            user.Entity.LastModifiedAt = DateTime.UtcNow;
        }

        await _appContext.SaveChangesAsync();
    }
}