using UserManagement.Application.Contexts;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Interfaces;

public interface IUserService
{
    Task<Result<User>> LoginAsync(LoginContext context);
    Task<Result<User>> RegisterAsync(RegistrationContext context);
    Task<Result<User>> GetAsync(Guid id);
    Task<Result<User>> UpdateAsync(UpdateUserContext context);
    Task<Result> DeactivateAsync(Guid userId);
    Task<Result> ReactivateAsync(Guid userId);
    Task<Result> DeleteAsync(DeleteUserContext context);
    Task<Result> DeleteByAdminAsync(Guid userId);
}