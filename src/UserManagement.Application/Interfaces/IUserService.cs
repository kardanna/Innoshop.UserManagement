using UserManagement.Application.Contexts;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Interfaces;

public interface IUserService
{
    Task<Result<User>> LoginAsync(LoginContext context);
    Task<Result<Guid>> RegisterAsync(RegistrationContext context);
    Task<Result<User>> GetUserAsync(Guid id);
}