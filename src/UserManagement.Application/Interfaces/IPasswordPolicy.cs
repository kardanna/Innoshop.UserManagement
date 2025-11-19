using UserManagement.Application.Contexts;
using UserManagement.Application.Models;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IPasswordPolicy
{
    Task<PolicyResult> IsPasswordChangeAllowedAsync(User user, ChangePasswordContext context);
    Task<PolicyResult> IsPasswordRestoreAllowed(PasswordRestoreAttempt attempt);
}