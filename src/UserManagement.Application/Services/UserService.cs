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
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserPolicy _userPolicy;
    private readonly IInnoshopNotifier _innoshopNotifier;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<User> hasher,
        IUnitOfWork unitOfWork,
        IUserPolicy userPolicy,
        IInnoshopNotifier innoshopNotifier)
    {
        _userRepository = userRepository;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
        _userPolicy = userPolicy;
        _innoshopNotifier = innoshopNotifier;
    }

    public async Task<Result<User>> LoginAsync(LoginContext context)
    {
        var attempt = await _userPolicy.IsLoginAllowedAsync(context);

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

        return user;
    }

    public async Task<Result<User>> RegisterAsync(RegistrationContext context)
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
            Roles = context.Roles,
            IsDeactivated = true
        };

        _userRepository.Add(user);

        //Save on DB context is performed inside Email service after sending verification email 

        return user;
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

    public async Task<Result> DeactivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetAsync(userId);

        if (user is null) return Result.Failure(DomainErrors.User.NotFound);

        if (user.IsDeactivated) return Result.Success();

        user.IsDeactivated = true;
        user.DeactivationRequestedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        await _innoshopNotifier.SendUserDeactivatedNotificationAsync(new()
            {
                UserId = user.Id,
                DeactivatedAtUtc = DateTime.UtcNow
            }
        );

        return Result.Success();
    }

    public async Task<Result> ReactivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetAsync(userId);

        if (user is null) return Result.Failure(DomainErrors.User.NotFound);

        if (!user.IsDeactivated) return Result.Success();

        user.IsDeactivated = false;
        user.DeactivationRequestedAt = null;
        await _unitOfWork.SaveChangesAsync();

        await _innoshopNotifier.SendUserReactivatedNotificationAsync(new()
            {
                UserId = user.Id,
                ReactivatedAtUtc = DateTime.UtcNow
            }
        );

        return Result.Success();
    }
}