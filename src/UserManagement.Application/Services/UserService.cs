using Microsoft.AspNetCore.Identity;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoginPolicy _loginPolicy;
    private readonly IUserPolicy _userPolicy;

    public UserService(
        IEmailService emailService,
        IUserRepository userRepository,
        IPasswordHasher<User> hasher,
        IUnitOfWork unitOfWork,
        ILoginPolicy loginPolicy,
        IUserPolicy userPolicy)
    {
        _emailService = emailService;
        _userRepository = userRepository;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
        _loginPolicy = loginPolicy;
        _userPolicy = userPolicy;
    }

    public async Task<bool> IsEmailAvailable(string email)
    {
        return await _userRepository.CountUsersWithEmailAsync(email) == 0;
    }

    public async Task<Result<User>> LoginAsync(LoginContext context)
    {
        var attempt = await _loginPolicy.IsLoginAllowedAsync(context);

        if (!attempt.IsAllowed)
        {
            return attempt.Error;
        }
        
        var user = await _userRepository.GetAsync(context.Email);

        if (user == null)
        {
            return DomainErrors.Login.WrongEmailOrPassword;
        }
        
        var passwordMatch = _hasher.VerifyHashedPassword(null!, user.PasswordHash, context.Password);

        if (passwordMatch == PasswordVerificationResult.Failed)
        {
            return DomainErrors.Login.WrongEmailOrPassword;
        }

        if (!user.IsEmailVerified)
        {
            return DomainErrors.Login.EmailUnverified;
        }

        //await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<Result<Guid>> RegisterAsync(RegistrationContext context)
    {
        var attempt = await _userPolicy.IsRegistrationAllowedAsync(context);

        if (!attempt.IsAllowed)
        {
            return attempt.Error;
        }

        var user = new User()
        {
            FirstName = context.FirstName,
            LastName = context.LastName,
            DateOfBirth = context.DateOfBirth,
            Email = context.Email,
            PasswordHash = _hasher.HashPassword(null!, context.Password),
            Roles = []
        };

        await _emailService.SendRequestToVerifyUserEmailAsync(user);

        await _unitOfWork.SaveChangesAsync();

        return user.Id;
    }

    public async Task<Result<User>> GetUserAsync(Guid id)
    {
        var user = await _userRepository.GetAsync(id);

        if (user == null) return DomainErrors.User.NotFound;

        return Result.Success(user);
    }

    public async Task<Result<User>> UpdateUserAsync(UpdateUserContext context)
    {
        var user = await GetUserAsync(context.UserId);

        if (user.IsFailure) return user.Error;

        var attempt = await _userPolicy.IsUpdateAllowedAsync(context);

        if (attempt.IsDenied) return attempt.Error;

        user.Value.FirstName = context.FirstName;
        user.Value.LastName = context.LastName;
        user.Value.DateOfBirth = context.DateOfBirth;

        await _unitOfWork.SaveChangesAsync();

        return user;
    }
}