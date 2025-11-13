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

    public EmailPolicy(
        IOptions<EmailOptions> options,
        IUserRepository userRepository,
        IEmailVerificationAttemptRepository emailVerificationAttemptRepository)
    {
        _options = options.Value;
        _userRepository = userRepository;
        _emailVerificationAttemptRepository = emailVerificationAttemptRepository;
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

        if (!isEmailStillAvailable) return DomainErrors.EmailChange.EmailAlreadyInUse;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsEmailChangeAllowed(EmailChangeContext context)
    {
        var isTheSameEmail = context.User.Email == context.NewEmail;

        if (isTheSameEmail) return DomainErrors.EmailChange.TheSameEmail;

        var isEmailAvailable = await _userRepository.CountUsersWithEmailAsync(context.NewEmail) == 0;

        if (!isEmailAvailable) return DomainErrors.EmailChange.EmailAlreadyInUse;

        var lastAttempt = await _emailVerificationAttemptRepository
            .GetLastSuccessfulAttemptAsync(context.NewEmail);
        
        if (lastAttempt == null) return PolicyResult.Success;

        var isTooOften = lastAttempt.AttemptedAt >
            DateTime.UtcNow.AddHours(-_options.UserCanChangeEmailOnceInHowManyHours);

        if (isTooOften) return DomainErrors.EmailChange.TooOften;

        return PolicyResult.Success;
    }
}