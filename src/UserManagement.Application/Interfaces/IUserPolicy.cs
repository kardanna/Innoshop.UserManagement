using UserManagement.Application.Contexts;
using UserManagement.Application.Models;

namespace UserManagement.Application.Interfaces;

public interface IRegistrationPolicy
{
    Task<PolicyResult> IsRegistrationAllowedAsync(RegistrationContext context);
}