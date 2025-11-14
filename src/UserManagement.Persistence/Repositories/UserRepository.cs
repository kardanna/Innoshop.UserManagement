using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationContext _appContext;

    public UserRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task<User?> GetAsync(Guid id)
    {
        return await _appContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetAsync(string email)
    {
        return await _appContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<int> CountUsersWithEmailAsync(string email)
    {
        return await _appContext.Users
            .Where(u => u.Email == email)
            .CountAsync();
    }
}