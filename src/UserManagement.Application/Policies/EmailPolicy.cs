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
        var isExpiredCode = attempt.AttemptedAt < DateTime.UtcNow.AddHours(-_options.VerificationCodeLifetimeInHours);
        if (isExpiredCode)
        {
            return DomainErrors.EmailVerification.CodeExpiredOrNotFound;
        }

        if (attempt.PreviousEmail == null) return PolicyResult.Success; //means it's not a change but initial confirmation

        var isEmailStillAvailable = await _userRepository.CountUsersWithEmailAsync(attempt.Email) == 0;

        if (!isEmailStillAvailable) return DomainErrors.Email.EmailAlreadyInUse;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsEmailChangeAllowed(EmailChangeContext context)
    {
        if (await IsUserDeacivated(context.User.Id)) return DomainErrors.User.Deactivated;

        var isTheSameEmail = context.User.Email == context.NewEmail;

        if (isTheSameEmail) return DomainErrors.EmailChange.TheSameEmail;

        var isEmailAvailable = await _userRepository.CountUsersWithEmailAsync(context.NewEmail) == 0;

        if (!isEmailAvailable) return DomainErrors.Email.EmailAlreadyInUse;

        var lastAttempt = await _emailVerificationAttemptRepository
            .GetLastSuccessfulAttemptAsync(context.NewEmail);
        
        if (lastAttempt is null) return PolicyResult.Success;

        var isTooOften = lastAttempt.AttemptedAt
            > DateTime.UtcNow.AddHours(-_options.UserCanChangeEmailOnceInHowManyHours);

        if (isTooOften) return DomainErrors.EmailChange.TooOften;

        return PolicyResult.Success;
    }

    private async Task<bool> IsUserDeacivated(Guid userId)
    {
        var lastDeactivationRecord = await _userDeactivationRepository.GetLatestAsync(userId);
        return lastDeactivationRecord is not null && lastDeactivationRecord.ReactivatedAt is null;
    }
}