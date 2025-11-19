using System.Security.Cryptography;
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
    private readonly IPasswordRestoreAttemptRepository _passwordRestoreAttemptRepository;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserPolicy _userPolicy;
    private readonly IPasswordPolicy _passwordPolicy;
    private readonly IInnoshopNotifier _innoshopNotifier;

    public UserService(
        IUserRepository userRepository,
        IUserDeactivationRepository userDeactivationRepository,
        IPasswordRestoreAttemptRepository passwordRestoreAttemptRepository,
        IPasswordHasher<User> hasher,
        IUnitOfWork unitOfWork,
        IUserPolicy userPolicy,
        IPasswordPolicy passwordPolicy,
        IInnoshopNotifier innoshopNotifier)
    {
        _userRepository = userRepository;
        _userDeactivationRepository = userDeactivationRepository;
        _passwordRestoreAttemptRepository = passwordRestoreAttemptRepository;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
        _userPolicy = userPolicy;
        _passwordPolicy = passwordPolicy;
        _innoshopNotifier = innoshopNotifier;
    }

    private const string DEACTIVATED_BY_HIMSELF_COMMENTARY = "Request issued by user";
    private const string DEACTIVATED_BY_ADMIN_COMMENTARY = "Request issued by administrator";
    private const string PLACEHOLDER_FOR_DELETED_USER = "USER DELETED";

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

    public async Task<Result<User>> GetAsync(string email)
    {
        var user = await _userRepository.GetAsync(email);

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
        var subject = await _userRepository.GetAsync(context.SubjectId);

        if (subject is null) return Result.Failure(DomainErrors.User.NotFound);

        User? requester;

        if (context.SubjectId == context.RequesterId)
        {
            requester = subject;
        }
        else
        {
            requester = await _userRepository.GetAsync(context.RequesterId);
            if (requester is null) return Result.Failure(DomainErrors.User.NotFound);
        }
        
        var attempt = await _userPolicy.IsDeletionAllowedAsync(subject, requester, context);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        subject.FirstName = PLACEHOLDER_FOR_DELETED_USER;
        subject.LastName = PLACEHOLDER_FOR_DELETED_USER;
        subject.DateOfBirth = default;
        subject.Email = PLACEHOLDER_FOR_DELETED_USER;
        subject.IsDeleted = true;
        subject.DeletionRequestedAt = DateTime.UtcNow;

        _userDeactivationRepository.RemoveAllForUser(context.SubjectId);

        //await _unitOfWork.SaveChangesAsync(); //Changes are saved in email service after clearing up user records;

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordContext context)
    {
        var user = await _userRepository.GetAsync(context.UserId);
        if (user is null) return Result.Failure(DomainErrors.User.NotFound);

        var attempt = await _passwordPolicy.IsPasswordChangeAllowedAsync(user, context);
        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        user.PasswordHash = _hasher.HashPassword(null!, context.NewPassword);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<string>> InitiatePasswordRestorationAsync(string userEmail)
    {
        var user = await _userRepository.GetAsync(userEmail);

        if (user is null) return DomainErrors.User.NotFound;

        _passwordRestoreAttemptRepository.RemovePreviousUnseccessfulAttempts(user.Id);

        var attempt = new PasswordRestoreAttempt()
        {
            AttemptCode = GenerateRestorationCode(),
            User = user,
            AttemptedAt = DateTime.UtcNow
        };

        _passwordRestoreAttemptRepository.Add(attempt);

        await _unitOfWork.SaveChangesAsync();

        return attempt.AttemptCode;
    }

    public async Task<Result> RestorePasswordAsync(string restoreCode, string newPassword)
    {
        var restoreAttempt = await _passwordRestoreAttemptRepository.GetAsync(restoreCode);

        if (restoreAttempt is null) return Result.Failure(DomainErrors.PasswordRestore.InvalidOrExpiredRestoreCode);

        var attempt = await _passwordPolicy.IsPasswordRestoreAllowed(restoreAttempt);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        restoreAttempt.User.PasswordHash = _hasher.HashPassword(null!, newPassword);
        restoreAttempt.IsSucceeded = true;
        restoreAttempt.SucceededAt = DateTime.UtcNow;
        
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<bool> IsUserDeacivated(Guid userId)
    {
        var lastDeactivationRecord = await _userDeactivationRepository.GetLatestAsync(userId);
        return lastDeactivationRecord is not null && lastDeactivationRecord.ReactivatedAt is null;
    }

    private static string GenerateRestorationCode()
    {
        var randomNumber = new byte[256];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}