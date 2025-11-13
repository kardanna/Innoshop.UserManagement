using UserManagement.Application.Contexts;
using UserManagement.Application.Models;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IEmailPolicy
{
    Task<PolicyResult> IsConfirmationAllowedAsync(EmailVerificationAttempt attempt);
    Task<PolicyResult> IsEmailChangeAllowed(EmailChangeContext context);
}