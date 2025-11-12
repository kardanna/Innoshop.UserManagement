using Microsoft.Extensions.Options;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.Policies;

public class LoginAttemptPolicy : ILoginAttemptPolicy
{
    private readonly ILoginAttemptRepository _repository;
    private readonly LoginOptions _options;

    public LoginAttemptPolicy(
        ILoginAttemptRepository repository,
        IOptions<LoginOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<CanLoginResult> IsLoginAllowed(LoginContext context)
    {
        var numberOfAttempts = await _repository
            .CountLoginAttemptsAsync(context.Email, _options.LoginAttemptsTimeWindowInMinutes);

        if (numberOfAttempts > _options.LoginAttemptsMaxCount)
        {
            return CanLoginResult.Failure(DomainErrors.Login.TooManyAttempts);
        }

        //await _repository.RegisterAttemptAsync(email);
        
        return CanLoginResult.Success;
    }

    public void RegisterAttempt(LoginContext context)
    {
        _repository.AddAttempt(context.Email, context.DeviceFingerprint);
    }
}