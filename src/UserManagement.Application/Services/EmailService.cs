using System.Security.Cryptography;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Services;

public class EmailService : IEmailService
{
    private readonly IEmailVerificationAttemptRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailPolicy _emailPolicy;
    //private readonly IEmailSender _emailSender;

    public EmailService(
        IEmailVerificationAttemptRepository repository,
        IUnitOfWork unitOfWork,
        IEmailPolicy emailPolicy)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _emailPolicy = emailPolicy;
    }

    public async Task SendRequestToVerifyUserEmailAsync(User user)
    {
        var attempt = new EmailVerificationAttempt()
        {
            VerificationCode = GenerateVerificationCode(),
            User = user,
            Email = user.Email,
            AttemptedAt = DateTime.UtcNow
        };

        _repository.Add(attempt);

        //await _emailSender.SendVerificationEmail(attempt.Email, attempt.AttemptCode);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Result> SendRequestToChangeUserEmailAsync(EmailChangeContext context)
    {
        var attempt = await _emailPolicy.IsEmailChangeAllowed(context);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        var request = new EmailVerificationAttempt()
        {
            VerificationCode = GenerateVerificationCode(),
            User = context.User,
            Email = context.NewEmail,
            PreviousEmail = context.User.Email,
            AttemptedAt = DateTime.UtcNow
        };

        _repository.RemoveUnseccessfulAttemptsFor(context.User.Email);

        _repository.Add(request);

        //await _emailSender.SendVerificationEmail(attempt.Email, attempt.AttemptCode);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ConfirmSednedRequestAsync(string verificationCode)
    {
        var attemptRecord = await _repository.GetAsync(verificationCode);

        if (attemptRecord == null) return Result.Failure(DomainErrors.EmailVerification.CodeExpiredOrNotFound);

        var attempt = await _emailPolicy.IsConfirmationAllowedAsync(attemptRecord);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        attemptRecord.IsSucceeded = true;
        attemptRecord.SucceededAt = DateTime.UtcNow;
        
        if (attemptRecord.PreviousEmail is null)
        {
            attemptRecord.User.IsEmailVerified = true;
        }

        if (attemptRecord.PreviousEmail is not null)
        {
            attemptRecord.User.Email = attemptRecord.Email;
        }

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    private static string GenerateVerificationCode()
    {
        var randomNumber = new byte[256];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task ClearUserRecordsAsync(Guid userId)
    {
        _repository.RemoveAllUserAttempts(userId);
        await _unitOfWork.SaveChangesAsync();
    }
}