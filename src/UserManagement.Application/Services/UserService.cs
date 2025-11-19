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
    private readonly IUserDeactivationRepository _userDeactivationRepository;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserPolicy _userPolicy;
    private readonly IInnoshopNotifier _innoshopNotifier;

    public UserService(
        IUserRepository userRepository,
        IUserDeactivationRepository userDeactivationRepository,
        IPasswordHasher<User> hasher,
        IUnitOfWork unitOfWork,
        IUserPolicy userPolicy,
        IInnoshopNotifier innoshopNotifier)
    {
        _userRepository = userRepository;
        _userDeactivationRepository = userDeactivationRepository;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
        _userPolicy = userPolicy;
        _innoshopNotifier = innoshopNotifier;
    }

    private const string DEACTIVATED_BY_HIMSELF_COMMENTARY = "Request issued by user";
    private const string DEACTIVATED_BY_ADMIN_COMMENTARY = "Request issued by administrator";

    public async Task<Result<User>> LoginAsync(LoginUserContext context)
    {
        var user = await _userRepository.GetAsync(context.Email);

        if (user is null) return DomainErrors.Login.WrongEmailOrPassword;

        var attempt = await _userPolicy.IsLoginAllowedAsync(user, context);

        if (attempt.IsDenied) return attempt.Error;
        
        var passwordMatch = _hasher.VerifyHashedPassword(null!, user.PasswordHash, context.Password);

        if (passwordMatch == PasswordVerificationResult.Failed)
        {
            return DomainErrors.Login.WrongEmailOrPassword;
        }

        return user;
    }

    public async Task<Result<User>> RegisterAsync(RegistrationContext context)
    {
        var attempt = await _userPolicy.IsRegistrationAllowedAsync(context);

        if (attempt.IsDenied) return attempt.Error;

        var user = new User()
        {
            FirstName = context.FirstName,
            LastName = context.LastName,
            DateOfBirth = context.DateOfBirth,
            Email = context.Email,
            PasswordHash = _hasher.HashPassword(null!, context.Password),
            Roles = context.Roles
        };

        _userRepository.Add(user);

        //Save on DB context is performed inside Email service after sending verification email 

        return user;
    }

    public async Task<Result<User>> GetAsync(Guid id)
    {
        var user = await _userRepository.GetAsync(id);

        if (user is null) return DomainErrors.User.NotFound;

        return Result.Success(user);
    }

    public async Task<Result<User>> UpdateAsync(UpdateUserContext context)
    {
        var user = await _userRepository.GetAsync(context.UserId);

        if (user is null) return DomainErrors.User.NotFound;

        var attempt = await _userPolicy.IsUpdateAllowedAsync(user, context);

        if (attempt.IsDenied) return attempt.Error;

        user.FirstName = context.FirstName;
        user.LastName = context.LastName;
        user.DateOfBirth = context.DateOfBirth;

        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<Result> DeactivateAsync(Guid subjectId, Guid requesterId)
    {
        var subject = await _userRepository.GetAsync(subjectId);

        if (subject is null) return Result.Failure(DomainErrors.User.NotFound);

        User? requester;

        if (subjectId == requesterId)
        {
            requester = subject;
        }
        else
        {
            requester = await _userRepository.GetAsync(requesterId);
            if (requester is null) return Result.Failure(DomainErrors.User.NotFound);
        }

        var attempt = await _userPolicy.IsDeactivationAllowedAsync(subject, requester);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        var deactivationRecord = new UserDeactivation()
        {
            User = subject,
            DeactivatedAt = DateTime.UtcNow,
            DeactivationRequester = requester,
            Commentary = DEACTIVATED_BY_HIMSELF_COMMENTARY
        };

        if (requester.Roles.Any(r => r == Role.Administrator))
        {
            deactivationRecord.Commentary = DEACTIVATED_BY_ADMIN_COMMENTARY;
        }

        _userDeactivationRepository.Add(deactivationRecord);

        await _unitOfWork.SaveChangesAsync();

        await _innoshopNotifier.SendUserDeactivatedNotificationAsync(new()
            {
                UserId = subject.Id,
                DeactivatedAtUtc = deactivationRecord.DeactivatedAt
            }
        );

        return Result.Success();
    }

    public async Task<Result> ReactivateAsync(Guid subjectId, Guid requesterId)
    {
        var subject = await _userRepository.GetAsync(subjectId);

        if (subject is null) return Result.Failure(DomainErrors.User.NotFound);

        User? requester;

        if (subjectId == requesterId)
        {
            requester = subject;
        }
        else
        {
            requester = await _userRepository.GetAsync(requesterId);
            if (requester is null) return Result.Failure(DomainErrors.User.NotFound);
        }
        
        var attempt = await _userPolicy.IsReactivationAllowedAsync(subject, requester);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        var record = await _userDeactivationRepository.GetLatestAsync(subject.Id);
        if (record is null) return Result.Failure(DomainErrors.Reactivation.AlreadyReactivated);

        record.ReactivatedAt = DateTime.UtcNow;
        record.ReactivationRequester = requester;

        await _unitOfWork.SaveChangesAsync();

        await _innoshopNotifier.SendUserReactivatedNotificationAsync(new()
            {
                UserId = subject.Id,
                ReactivatedAtUtc = DateTime.UtcNow
            }
        );

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(DeleteUserContext context)
    {
        var user = await _userRepository.GetAsync(context.UserId);

        if (user is null) return Result.Failure(DomainErrors.User.NotFound);

        var passwordMatch = _hasher.VerifyHashedPassword(null!, user.PasswordHash, context.Password);

        if (passwordMatch == PasswordVerificationResult.Failed)
        {
            return Result.Failure(DomainErrors.Login.WrongEmailOrPassword);
        }

        var attempt = await _userPolicy.IsDeleteAllowedAsync(user);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        user.FirstName = "USER DELETED";
        user.LastName = "USER DELETED";
        user.DateOfBirth = default;
        user.Email = "USER DELETED";
        user.IsDeleted = true;
        user.DeletionRequestedAt = DateTime.UtcNow;

        _userDeactivationRepository.RemoveAllForUser(context.UserId);

        //await _unitOfWork.SaveChangesAsync(); //Changes are saved in email service after clearing up user records;

        return Result.Success();
    }

    public async Task<Result> DeleteByAdminAsync(Guid userId)
    {
        var user = await _userRepository.GetAsync(userId);

        if (user is null) return Result.Failure(DomainErrors.User.NotFound);

        user.FirstName = "USER DELETED";
        user.LastName = "USER DELETED";
        user.DateOfBirth = default;
        user.Email = "USER DELETED";
        user.IsDeleted = true;
        user.DeletionRequestedAt = DateTime.UtcNow;

        _userDeactivationRepository.RemoveAllForUser(userId);

        //await _unitOfWork.SaveChangesAsync(); //Changes are saved in email service after clearing up user records;

        return Result.Success();
    }

    public async Task<bool> IsUserDeacivated(Guid userId)
    {
        var lastDeactivationRecord = await _userDeactivationRepository.GetLatestAsync(userId);
        return lastDeactivationRecord is not null && lastDeactivationRecord.ReactivatedAt is null;
    }
}