using UserManagement.Domain.Entities;

namespace UserManagement.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id);
    Task<User?> GetAsync(string email);
    Task<int> CountUsersWithEmailAsync(string email);
}