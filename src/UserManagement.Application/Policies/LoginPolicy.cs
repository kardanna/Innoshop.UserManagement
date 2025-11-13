using Microsoft.Extensions.Options;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Models;
using UserManagement.Application.Options;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Errors;

namespace UserManagement.Application.Policies;

public class LoginPolicy : ILoginPolicy
{
    private readonly ILoginAttemptRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoginOptions _options;

    public LoginPolicy(
        ILoginAttemptRepository repository,
        IUnitOfWork unitOfWork,
        IOptions<LoginOptions> options)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<PolicyResult> IsLoginAllowedAsync(LoginContext context)
    {
        var numberOfAttempts = await _repository
            .CountLoginAttemptsAsync(context.Email, _options.LoginAttemptsTimeWindowInMinutes);

        if (numberOfAttempts > _options.LoginAttemptsMaxCount)
        {
            return DomainErrors.Login.TooManyAttempts;
        }

        RegisterAttempt(context);

        await _unitOfWork.SaveChangesAsync();
        
        return PolicyResult.Success;
    }

    private void RegisterAttempt(LoginContext context)
    {
        _repository.AddAttempt(context.Email, context.DeviceFingerprint);
    }
}