using UserManagement.Application.Contexts;
using UserManagement.Application.Models;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IUserPolicy
{
    Task<PolicyResult> IsRegistrationAllowedAsync(RegistrationContext context);
    Task<PolicyResult> IsUpdateAllowedAsync(UpdateUserContext context);
    Task<PolicyResult> IsLoginAllowedAsync(LoginContext context);
    Task<PolicyResult> IsDeleteAllowedAsync(User user);
}