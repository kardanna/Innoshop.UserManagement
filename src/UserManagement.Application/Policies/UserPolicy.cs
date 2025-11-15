using Microsoft.Extensions.Options;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.Policies;

public class UserPolicy : IUserPolicy
{
    private readonly IUserRepository _userRpository;
    private readonly ILoginAttemptRepository _loginRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegistrationOptions _registrationOptions;
    private readonly LoginOptions _loginOptions;

    public UserPolicy(
        IUserRepository userRpository,
        ILoginAttemptRepository loginRepository,
        IUnitOfWork unitOfWork,
        IOptions<RegistrationOptions> registrationOptions,
        IOptions<LoginOptions> loginOptions)
    {
        _userRpository = userRpository;
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

        var isEmailAvailable = await _userRpository.CountUsersWithEmailAsync(context.Email) == 0;

        if (!isEmailAvailable)
        {
            return DomainErrors.Register.EmailAlreadyInUse;
        }

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsUpdateAllowedAsync(UpdateUserContext context)
    {
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

        RegisterAttempt(context);

        await _unitOfWork.SaveChangesAsync();
        
        return PolicyResult.Success;
    }

    private void RegisterAttempt(LoginContext context)
    {
        _loginRepository.AddAttempt(context.Email, context.DeviceFingerprint);
    }
}