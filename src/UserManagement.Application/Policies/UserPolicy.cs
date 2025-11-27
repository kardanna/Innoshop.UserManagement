using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.Policies;

public class UserPolicy : IUserPolicy
{
    private readonly IUserRepository _userRepository;
    private readonly ILoginAttemptRepository _loginRepository;
    private readonly IUserDeactivationRepository _userDeactivationRepository;
    private readonly RegistrationOptions _registrationOptions;
    private readonly LoginOptions _loginOptions;
    private readonly IPasswordHasher<User> _hasher;

    public UserPolicy(
        IUserRepository userRepository,
        ILoginAttemptRepository loginRepository,
        IUserDeactivationRepository userDeactivationRepository,
        IOptions<RegistrationOptions> registrationOptions,
        IOptions<LoginOptions> loginOptions,
        IPasswordHasher<User> hasher)
    {
        _userRepository = userRepository;
        _loginRepository = loginRepository;
        _userDeactivationRepository = userDeactivationRepository;
        _registrationOptions = registrationOptions.Value;
        _loginOptions = loginOptions.Value;
        _hasher = hasher;
    }

    public async Task<PolicyResult> IsRegistrationAllowedAsync(RegistrationContext context)
    {
        if (!IsOfLegalAge(context.DateOfBirth)) return DomainErrors.Register.IllegalAge;

        if (!await IsEmailAvailable(context.Email)) return DomainErrors.Email.EmailAlreadyInUse;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsUpdateAllowedAsync(User user, UpdateUserContext context)
    {
        if (user.IsDeleted) return DomainErrors.User.NotFound;
        
        if (!user.IsEmailVerified) return DomainErrors.Email.EmailUnverified;

        if (!IsOfLegalAge(context.DateOfBirth)) return DomainErrors.Register.IllegalAge;

        if (await IsUserDeacivated(user.Id)) return DomainErrors.User.Deactivated;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsLoginAllowedAsync(User user, LoginUserContext context)
    {
        if (user.IsDeleted) return DomainErrors.User.NotFound;

        if (!user.IsEmailVerified) return DomainErrors.Email.EmailUnverified;

        if (!IsPasswordMatches(user, context.Password)) return DomainErrors.Login.WrongEmailOrPassword;
        
        if (await IsLoginAttemptsDepleted(context.Email)) return DomainErrors.Login.TooManyAttempts;
        
        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsDeactivationAllowedAsync(User subject, User requester)
    {
        if (subject.IsDeleted || requester.IsDeleted) return DomainErrors.User.NotFound;

        if (HasAdminRole(subject)) return DomainErrors.Deactivation.CannotDeactivateAdmin;

        if (!IsDeactivationRequesterAuthorized(subject, requester)) return DomainErrors.Deactivation.NotAdminRequester;
        
        if (await IsUserDeacivated(subject.Id)) return DomainErrors.Deactivation.AlreadyDeactivated;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsReactivationAllowedAsync(User subject, User requester, UserDeactivation record)
    {
        if (subject.IsDeleted || requester.IsDeleted) return DomainErrors.User.NotFound;

        if (HasAdminRole(subject)) return DomainErrors.Reactivation.CannotReactivateAdmin;
        
        if (!IsUserDeacivated(record)) return DomainErrors.Reactivation.AlreadyReactivated;

        if (!IsReactivationRequesterAuthorized(record!, requester)) return DomainErrors.Reactivation.NotAuthorized;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsDeletionAllowedAsync(User subject, User requester, DeleteUserContext context)
    {
        if (subject.IsDeleted || requester.IsDeleted) return DomainErrors.User.NotFound;

        if (subject != requester && HasAdminRole(requester)) return PolicyResult.Success;

        if (subject != requester && !HasAdminRole(requester)) return DomainErrors.Deletion.NotAdminRequester;

        if (string.IsNullOrWhiteSpace(context.Password)) return DomainErrors.Deletion.EmptyOrWrongPassword;

        if (!IsPasswordMatches(subject, context.Password)) return DomainErrors.Deletion.EmptyOrWrongPassword;

        return PolicyResult.Success;
    }

    private async Task<bool> IsUserDeacivated(Guid userId)
    {
        var lastDeactivationRecord = await _userDeactivationRepository.GetLatestAsync(userId);
        return lastDeactivationRecord is not null && lastDeactivationRecord.ReactivatedAt is null;
    }

    private bool IsUserDeacivated(UserDeactivation? record)
    {
        return record is not null && record.ReactivatedAt is null;
    }

    private bool HasAdminRole(User user)
    {
        return user.Roles.Any(r => r == Role.Administrator);
    }

    private bool IsOfLegalAge(DateOnly dob)
    {
        return dob < DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-_registrationOptions.MustBeAtLeastYears));
    }

    private async Task<bool> IsEmailAvailable(string email)
    {
        return await _userRepository.CountUsersWithEmailAsync(email) == 0;
    }

    private async Task<bool> IsLoginAttemptsDepleted(string email)
    {
        var numberOfAttempts = await _loginRepository
            .CountLoginAttemptsAsync(email, _loginOptions.LoginAttemptsTimeWindowInMinutes);
        
        return numberOfAttempts > _loginOptions.LoginAttemptsMaxCount;
    }

    private bool IsPasswordMatches(User user, string password)
    {
        var passwordMatch = _hasher.VerifyHashedPassword(null!, user.PasswordHash, password);

        if (passwordMatch == PasswordVerificationResult.Failed) return false;

        return true;
    }

    private bool IsDeactivationRequesterAuthorized(User subject, User requester)
    {
        return subject.Id == requester.Id || HasAdminRole(requester);
    }

    private bool IsReactivationRequesterAuthorized(UserDeactivation record, User requester)
    {
        return (record.UserId == requester.Id && !HasAdminRole(record.DeactivationRequester)) || HasAdminRole(requester);
    }
}