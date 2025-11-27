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
        if (user.IsDeleted) return DomainErrors.User.NotFound;
        
        if (IsPasswordDoesNotMatch(user, context.OldPassword)) return DomainErrors.PasswordChange.EmptyOrWrongPassword;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsPasswordRestoreAllowed(PasswordRestoreAttempt attempt)
    {        
        if (attempt.User.IsDeleted) return DomainErrors.User.NotFound;

        if (IsAttemptExpired(attempt)) return DomainErrors.PasswordRestore.InvalidOrExpiredRestoreCode;

        return PolicyResult.Success;
    }

    private bool IsPasswordDoesNotMatch(User user, string password)
    {
        return _hasher.VerifyHashedPassword(null!, user.PasswordHash, password) != PasswordVerificationResult.Success;
    }

    private bool IsAttemptExpired(PasswordRestoreAttempt attempt)
    {
        return attempt.AttemptedAt
            < DateTime.UtcNow.AddHours(-_passwordOptions.PasswordRestoreAttemptLifetimeInHours);
    }
}