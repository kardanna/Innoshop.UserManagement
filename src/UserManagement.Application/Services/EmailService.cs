using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Options;
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
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;
    private readonly IUrlProvider _urlProvider;

    public EmailService(
        IEmailVerificationAttemptRepository repository,
        IUnitOfWork unitOfWork,
        IEmailPolicy emailPolicy,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions,
        IUrlProvider urlProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _emailPolicy = emailPolicy;
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
        _urlProvider = urlProvider;
    }

    public async Task<Result> SendRequestToVerifyUserAccountAsync(User user)
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

        string? verificationUrl = _urlProvider.GetUrlForEmailVerificationEndpoint(request.VerificationCode);

        return await _emailSender.SendEmailAddressVerificationMessageAsync(request.Email, request.VerificationCode, verificationUrl);
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

    public async Task ClearUserRecordsAsync(Guid userId)
    {
        _repository.RemoveAllUserAttempts(userId);
    }

    public async Task<Result> SendPasswordResorationCode(string email, string code)
    {
        //var verificationUrl = $"{_emailOptions.PasswordRestoreCallbackUrl}/{code}";

        return await _emailSender.SendPasswordRestorationMessageAsync(email, code);
    }
}