using UserManagement.Application.Contexts;
using UserManagement.Application.Models;

namespace UserManagement.Application.Interfaces;

public interface ILoginPolicy
{
    Task<PolicyResult> IsLoginAllowedAsync(LoginContext context);
}