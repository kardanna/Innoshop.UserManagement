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
    private readonly IEmailPolicy _emailPolicy;
    private readonly IEmailSender _emailSender;
    private readonly IUrlProvider _urlProvider;

    public EmailService(
        IEmailVerificationAttemptRepository repository,
        IEmailPolicy emailPolicy,
        IEmailSender emailSender,
        IUrlProvider urlProvider)
    {
        _repository = repository;
        _emailPolicy = emailPolicy;
        _emailSender = emailSender;
        _urlProvider = urlProvider;
    }

    public async Task<Result> VerifyAccountAsync(User user)
    {
        var request = new EmailVerificationAttempt()
        {
            VerificationCode = GenerateVerificationCode(),
            User = user,
            Email = user.Email,
            AttemptedAt = DateTime.UtcNow
        };

        _repository.Add(request);

        string? verificationUrl = _urlProvider.GetUrlForEmailVerificationEndpoint(request.VerificationCode);

        return await _emailSender.SendAccountVerificationMessageAsync(request.Email, request.VerificationCode, verificationUrl);
    }

    public async Task<Result> ChangeEmailAsync(EmailChangeContext context)
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

        string? verificationUrl = _urlProvider.GetUrlForEmailVerificationEndpoint(request.VerificationCode);

        return await _emailSender.SendEmailAddressVerificationMessageAsync(request.Email, request.VerificationCode, verificationUrl);
    }

    public async Task<Result> ConfirmRequestAsync(string verificationCode)
    {
        var attemptRecord = await _repository.GetAsync(verificationCode);

        if (attemptRecord is null) return Result.Failure(DomainErrors.EmailVerification.CodeExpiredOrNotFound);

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

        return Result.Success();
    }

    public async Task<Result> SendPasswordRestoreCode(string email, string code)
    {
        string? endpoint = _urlProvider.GetUrlForPasswordRestoreEndpoint();

        return await _emailSender.SendPasswordRestorationMessageAsync(email, code, endpoint);
    }

    public async Task ClearUserRecordsAsync(Guid userId)
    {
        _repository.RemoveAllUserAttempts(userId);
    }

    private static string GenerateVerificationCode(int size = 32)
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