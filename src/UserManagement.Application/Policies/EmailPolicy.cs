using Microsoft.Extensions.Options;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.Policies;

public class EmailPolicy : IEmailPolicy
{
    private readonly EmailOptions _options;
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationAttemptRepository _emailVerificationAttemptRepository;
    private readonly IUserDeactivationRepository _userDeactivationRepository;

    public EmailPolicy(
        IOptions<EmailOptions> options,
        IUserRepository userRepository,
        IEmailVerificationAttemptRepository emailVerificationAttemptRepository,
        IUserDeactivationRepository userDeactivationRepository)
    {
        _options = options.Value;
        _userRepository = userRepository;
        _emailVerificationAttemptRepository = emailVerificationAttemptRepository;
        _userDeactivationRepository = userDeactivationRepository;
    }

    public async Task<PolicyResult> IsConfirmationAllowedAsync(EmailVerificationAttempt attempt)
    {
        if (attempt.IsSucceeded) return DomainErrors.EmailVerification.CodeExpiredOrNotFound;
        
        if (IsVerificationCodeExpired(attempt)) return DomainErrors.EmailVerification.CodeExpiredOrNotFound;

        var isEmailChangeAttempt = attempt.User.Email != attempt.Email;
        if (isEmailChangeAttempt && !await IsEmailAvailable(attempt.Email)) return DomainErrors.Email.EmailAlreadyInUse;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsEmailChangeAllowed(EmailChangeContext context)
    {
        if (context.User.IsDeleted) return DomainErrors.User.NotFound;

        if (await IsUserDeacivated(context.User.Id)) return DomainErrors.User.Deactivated;

        var isTheSameEmail = context.User.Email == context.NewEmail;

        if (isTheSameEmail) return DomainErrors.EmailChange.TheSameEmail;

        if (!await IsEmailAvailable(context.NewEmail)) return DomainErrors.Email.EmailAlreadyInUse;

        var lastAttempt = await _emailVerificationAttemptRepository.GetLastSuccessfulAttemptAsync(context.User.Email);

        if (IsTooManyAttempts(lastAttempt)) return DomainErrors.EmailChange.TooOften;

        return PolicyResult.Success;
    }

    private async Task<bool> IsUserDeacivated(Guid userId)
    {
        var lastDeactivationRecord = await _userDeactivationRepository.GetLatestAsync(userId);
        return lastDeactivationRecord is not null && lastDeactivationRecord.ReactivatedAt is null;
    }

    private bool IsVerificationCodeExpired(EmailVerificationAttempt attempt)
    {
        return attempt.AttemptedAt < DateTime.UtcNow.AddHours(-_options.VerificationCodeLifetimeInHours);
    }

    private async Task<bool> IsEmailAvailable(string email)
    {
        return await _userRepository.CountUsersWithEmailAsync(email) == 0;
    }

    private bool IsTooManyAttempts(EmailVerificationAttempt? attempt)
    {
        return attempt is not null && attempt.AttemptedAt
            > DateTime.UtcNow.AddHours(-_options.UserCanChangeEmailOnceInHowManyHours);
    }
}