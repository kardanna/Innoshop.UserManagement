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
    private readonly IUserRepository _repository;
    private readonly RegistrationOptions _options;

    public UserPolicy(
        IUserRepository repository,
        IOptions<RegistrationOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<PolicyResult> IsRegistrationAllowedAsync(RegistrationContext context)
    {
        var isOfLegalAge = context.DateOfBirth <
            DateOnly.FromDateTime(DateTime.Now.AddYears(-_options.MustBeAtLeastYears));

        if (!isOfLegalAge)
        {
            return DomainErrors.Register.IllegalAge;
        }

        var isEmailAvailable = await _repository.CountUsersWithEmailAsync(context.Email) == 0;

        if (!isEmailAvailable)
        {
            return DomainErrors.Register.EmailAlreadyInUse;
        }

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsUpdateAllowedAsync(UpdateUserContext context)
    {
        var isOfLegalAge = context.DateOfBirth <
            DateOnly.FromDateTime(DateTime.Now.AddYears(-_options.MustBeAtLeastYears));

        if (!isOfLegalAge)
        {
            return DomainErrors.Register.IllegalAge;
        }

        return PolicyResult.Success;
    }
}