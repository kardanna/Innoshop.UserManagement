using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationContext _appContext;

    public UnitOfWork(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task SaveChangesAsync()
    {
        await _appContext.SaveChangesAsync();
    }
}