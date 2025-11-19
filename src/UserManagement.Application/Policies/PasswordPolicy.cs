using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.Policies;

public class PasswordPolicy : IPasswordPolicy
{
    private readonly Options.PasswordOptions _passwordOptions;
    private readonly IPasswordHasher<User> _hasher;

    public PasswordPolicy(
        IOptions<Options.PasswordOptions> passwordOptions,
        IPasswordHasher<User> hasher)
    {
        _passwordOptions = passwordOptions.Value;
        _hasher = hasher;
    }

    public async Task<PolicyResult> IsPasswordChangeAllowedAsync(User user, ChangePasswordContext context)
    {
        var passwordMatch = _hasher.VerifyHashedPassword(null!, user.PasswordHash, context.OldPassword);

        if (passwordMatch == PasswordVerificationResult.Failed) return DomainErrors.PasswordChange.EmptyOrWrongPassword;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsPasswordRestoreAllowed(PasswordRestoreAttempt attempt)
    {
        var isAttemptExpired = attempt.AttemptedAt
            < DateTime.UtcNow.AddHours(-_passwordOptions.PasswordRestoreAttemptLifetimeInHours);
        
        if (isAttemptExpired) return DomainErrors.PasswordRestore.InvalidOrExpiredRestoreCode;

        return PolicyResult.Success;
    }
}