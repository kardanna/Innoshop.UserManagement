using UserManagement.Application.Contexts;
using UserManagement.Application.Models;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IUserPolicy
{
    Task<PolicyResult> IsRegistrationAllowedAsync(RegistrationContext context);
    Task<PolicyResult> IsUpdateAllowedAsync(User user, UpdateUserContext context);
    Task<PolicyResult> IsLoginAllowedAsync(User user, LoginUserContext context);
    Task<PolicyResult> IsDeactivationAllowedAsync(User subject, User requester);
    Task<PolicyResult> IsReactivationAllowedAsync(User subject, User requester);
    Task<PolicyResult> IsDeleteAllowedAsync(User user);
}