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
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegistrationOptions _registrationOptions;
    private readonly LoginOptions _loginOptions;
    private readonly IPasswordHasher<User> _hasher;

    public UserPolicy(
        IUserRepository userRepository,
        ILoginAttemptRepository loginRepository,
        IUserDeactivationRepository userDeactivationRepository,
        IUnitOfWork unitOfWork,
        IOptions<RegistrationOptions> registrationOptions,
        IOptions<LoginOptions> loginOptions,
        IPasswordHasher<User> hasher)
    {
        _userRepository = userRepository;
        _loginRepository = loginRepository;
        _userDeactivationRepository = userDeactivationRepository;
        _unitOfWork = unitOfWork;
        _registrationOptions = registrationOptions.Value;
        _loginOptions = loginOptions.Value;
        _hasher = hasher;
    }

    public async Task<PolicyResult> IsRegistrationAllowedAsync(RegistrationContext context)
    {
        var isOfLegalAge = context.DateOfBirth
            < DateOnly.FromDateTime(DateTime.Now.AddYears(-_registrationOptions.MustBeAtLeastYears));
        if (!isOfLegalAge) return DomainErrors.Register.IllegalAge;

        var isEmailAvailable = await _userRepository.CountUsersWithEmailAsync(context.Email) == 0;
        if (!isEmailAvailable) return DomainErrors.Email.EmailAlreadyInUse;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsUpdateAllowedAsync(User user, UpdateUserContext context)
    {
        if (!user.IsEmailVerified) return DomainErrors.Email.EmailUnverified;

        if (await IsUserDeacivated(user.Id)) return DomainErrors.User.Deactivated;

        var isOfLegalAge = context.DateOfBirth
            < DateOnly.FromDateTime(DateTime.Now.AddYears(-_registrationOptions.MustBeAtLeastYears));
        if (!isOfLegalAge) return DomainErrors.Register.IllegalAge;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsLoginAllowedAsync(User user, LoginUserContext context)
    {
        if (!user.IsEmailVerified) return DomainErrors.Email.EmailUnverified;
        
        var numberOfAttempts = await _loginRepository
            .CountLoginAttemptsAsync(user.Email, _loginOptions.LoginAttemptsTimeWindowInMinutes);
        
        var tooMuchAttempts = numberOfAttempts > _loginOptions.LoginAttemptsMaxCount;
        if (tooMuchAttempts) return DomainErrors.Login.TooManyAttempts;

        RegisterLogginAttempt(user.Email, context.DeviceFingerprint);

        await _unitOfWork.SaveChangesAsync();
        
        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsDeactivationAllowedAsync(User subject, User requester)
    {       
        if (HasAdminRole(subject)) return DomainErrors.Deactivation.CannotDeactivateAdmin;

        var notAuthorized = subject.Id != requester.Id && !HasAdminRole(requester);
        if (notAuthorized) return DomainErrors.Deactivation.NotAdminRequester;
        
        if (await IsUserDeacivated(subject.Id)) return DomainErrors.Deactivation.AlreadyDeactivated;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsReactivationAllowedAsync(User subject, User requester)
    {
        if (HasAdminRole(subject)) return DomainErrors.Reactivation.CannotReactivateAdmin;
        
        var record = await _userDeactivationRepository.GetLatestAsync(subject.Id);
        
        var isSubjectDeactivated = record is not null && record.ReactivatedAt is null;
        
        if (!isSubjectDeactivated) return DomainErrors.Reactivation.AlreadyReactivated;

        var notAuthorized = HasAdminRole(record!.DeactivationRequester) && !HasAdminRole(requester);
        if (notAuthorized) return DomainErrors.Reactivation.NotAdminRequester;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsDeletionAllowedAsync(User subject, User requester, DeleteUserContext context)
    {
        if (subject != requester && HasAdminRole(requester)) return PolicyResult.Success;

        if (subject != requester && !HasAdminRole(requester)) return DomainErrors.Deletion.NotAdminRequester;

        if (context.Password is null) return DomainErrors.Deletion.EmptyOrWrongPassword;

        var passwordMatch = _hasher.VerifyHashedPassword(null!, subject.PasswordHash, context.Password);

        if (passwordMatch == PasswordVerificationResult.Failed)
        {
            return DomainErrors.Deletion.EmptyOrWrongPassword;
        }

        return PolicyResult.Success;
    }

    private void RegisterLogginAttempt(string email, string deviceFingerprint)
    {
        _loginRepository.AddAttempt(email, deviceFingerprint);
    }

    private async Task<bool> IsUserDeacivated(Guid userId)
    {
        var lastDeactivationRecord = await _userDeactivationRepository.GetLatestAsync(userId);
        return lastDeactivationRecord is not null && lastDeactivationRecord.ReactivatedAt is null;
    }

    private bool HasAdminRole(User user)
    {
        return user.Roles.Any(r => r == Role.Administrator);
    }
}