using System.Security.Cryptography;
using Innoshop.Contracts.UserManagement.UserRoles;
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
    private readonly ILoginAttemptRepository _loginRepository;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IUserPolicy _userPolicy;
    private readonly IPasswordPolicy _passwordPolicy;

    public UserService(
        IUserRepository userRepository,
        IUserDeactivationRepository userDeactivationRepository,
        IPasswordRestoreAttemptRepository passwordRestoreAttemptRepository,
        ILoginAttemptRepository loginRepository,
        IPasswordHasher<User> hasher,
        IUserPolicy userPolicy,
        IPasswordPolicy passwordPolicy)
    {
        _userRepository = userRepository;
        _userDeactivationRepository = userDeactivationRepository;
        _loginRepository = loginRepository;
        _passwordRestoreAttemptRepository = passwordRestoreAttemptRepository;
        _hasher = hasher;
        _userPolicy = userPolicy;
        _passwordPolicy = passwordPolicy;
    }

    private const string DEACTIVATED_BY_HIMSELF_COMMENTARY = "Request issued by user";
    private const string DEACTIVATED_BY_ADMIN_COMMENTARY = "Request issued by administrator";
    private const string PLACEHOLDER_FOR_DELETED_USER = "USER DELETED";

    public async Task<Result<User>> LoginAsync(LoginUserContext context)
    {
        _loginRepository.AddAttempt(context.Email, context.DeviceFingerprint);

        var user = await _userRepository.GetAsync(context.Email);

        if (user is null) return DomainErrors.Login.WrongEmailOrPassword;

        var attempt = await _userPolicy.IsLoginAllowedAsync(user, context);

        if (attempt.IsDenied) return attempt.Error;

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

        var record = await _userDeactivationRepository.GetLatestAsync(subject.Id);
        if (record is null) return Result.Failure(DomainErrors.Reactivation.AlreadyReactivated);
        
        var attempt = await _userPolicy.IsReactivationAllowedAsync(subject, requester, record);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        record.ReactivatedAt = DateTime.UtcNow;
        record.ReactivationRequester = requester;

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

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordContext context)
    {
        var user = await _userRepository.GetAsync(context.UserId);

        if (user is null) return Result.Failure(DomainErrors.User.NotFound);

        var attempt = await _passwordPolicy.IsPasswordChangeAllowedAsync(user, context);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        user.PasswordHash = _hasher.HashPassword(null!, context.NewPassword);

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

        return attempt.AttemptCode;
    }

    public async Task<Result<Guid>> RestorePasswordAsync(string restoreCode, string newPassword)
    {
        var restoreAttempt = await _passwordRestoreAttemptRepository.GetAsync(restoreCode);

        if (restoreAttempt is null) return DomainErrors.PasswordRestore.InvalidOrExpiredRestoreCode;

        var attempt = await _passwordPolicy.IsPasswordRestoreAllowed(restoreAttempt);

        if (attempt.IsDenied) return attempt.Error;

        restoreAttempt.User.PasswordHash = _hasher.HashPassword(null!, newPassword);
        restoreAttempt.IsSucceeded = true;
        restoreAttempt.SucceededAt = DateTime.UtcNow;

        return restoreAttempt.User.Id;
    }

    public async Task<bool> IsDeacivated(Guid userId)
    {
        var lastDeactivationRecord = await _userDeactivationRepository.GetLatestAsync(userId);
        return lastDeactivationRecord is not null && lastDeactivationRecord.ReactivatedAt is null;
    }

    private static string GenerateRestorationCode(int size = 32)
    {
        var randomNumber = new byte[size];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);
        var base64 = Convert.ToBase64String(randomNumber)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return base64;
    }
}