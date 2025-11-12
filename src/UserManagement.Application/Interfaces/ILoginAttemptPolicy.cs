using UserManagement.Application.Contexts;
using UserManagement.Application.Models;

namespace UserManagement.Application.Interfaces;

public interface ILoginAttemptPolicy
{
    Task<CanLoginResult> IsLoginAllowed(LoginContext context);
    void RegisterAttempt(LoginContext context);
}