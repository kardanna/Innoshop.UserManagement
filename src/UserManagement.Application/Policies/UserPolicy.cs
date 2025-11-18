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
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegistrationOptions _registrationOptions;
    private readonly LoginOptions _loginOptions;

    public UserPolicy(
        IUserRepository userRepository,
        ILoginAttemptRepository loginRepository,
        IUnitOfWork unitOfWork,
        IOptions<RegistrationOptions> registrationOptions,
        IOptions<LoginOptions> loginOptions)
    {
        _userRepository = userRepository;
        _loginRepository = loginRepository;
        _unitOfWork = unitOfWork;
        _registrationOptions = registrationOptions.Value;
        _loginOptions = loginOptions.Value;
    }

    public async Task<PolicyResult> IsRegistrationAllowedAsync(RegistrationContext context)
    {
        var isOfLegalAge = context.DateOfBirth <
            DateOnly.FromDateTime(DateTime.Now.AddYears(-_registrationOptions.MustBeAtLeastYears));

        if (!isOfLegalAge)
        {
            return DomainErrors.Register.IllegalAge;
        }

        var isEmailAvailable = await _userRepository.CountUsersWithEmailAsync(context.Email) == 0;

        if (!isEmailAvailable)
        {
            return DomainErrors.Register.EmailAlreadyInUse;
        }

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsUpdateAllowedAsync(UpdateUserContext context)
    {
        var user = await _userRepository.GetAsync(context.UserId);

        if (user is null || user.IsDeleted) return DomainErrors.User.NotFound;

        if (user.IsDeactivated) return DomainErrors.User.Deactivated;

        var isOfLegalAge = context.DateOfBirth <
            DateOnly.FromDateTime(DateTime.Now.AddYears(-_registrationOptions.MustBeAtLeastYears));

        if (!isOfLegalAge)
        {
            return DomainErrors.Register.IllegalAge;
        }

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsLoginAllowedAsync(LoginContext context)
    {
        var numberOfAttempts = await _loginRepository
            .CountLoginAttemptsAsync(context.Email, _loginOptions.LoginAttemptsTimeWindowInMinutes);

        if (numberOfAttempts > _loginOptions.LoginAttemptsMaxCount)
        {
            return DomainErrors.Login.TooManyAttempts;
        }

        RegisterLogginAttempt(context);

        await _unitOfWork.SaveChangesAsync();
        
        return PolicyResult.Success;
    }

    private void RegisterLogginAttempt(LoginContext context)
    {
        _loginRepository.AddAttempt(context.Email, context.DeviceFingerprint);
    }

    public async Task<PolicyResult> IsDeleteAllowedAsync(User user)
    {
        return PolicyResult.Success;
    }
}