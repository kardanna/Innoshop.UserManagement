using System.Security.Cryptography;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IEmailVerificationAttemptRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    //private readonly IEmailSender _emailSender;

    public EmailVerificationService(
        IEmailVerificationAttemptRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task SendRequestToVerifyCurrentEmailAsync(User user)
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

        //await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendRequestToVerifyNewEmailAsync(User user, string newEmail)
    {
        var attempt = new EmailVerificationAttempt()
        {
            VerificationCode = GenerateVerificationCode(),
            User = user,
            Email = newEmail,
            PreviousEmail = user.Email,
            AttemptedAt = DateTime.UtcNow
        };

        _repository.Add(attempt);

        //await _emailSender.SendVerificationEmail(attempt.Email, attempt.AttemptCode);

        //await _unitOfWork.SaveChangesAsync(); //UNCOMMENT???
    }

    public async Task VerifyFromRequest(string verificationCode)
    {
        var attempt = await _repository.GetAsync(verificationCode);

        if (attempt == null) return;

        attempt.IsSucceeded = true;
        attempt.SucceededAt = DateTime.UtcNow;
        attempt.User.IsEmailVerified = true;
        attempt.User.LastModifiedAt = DateTime.UtcNow;

        if (attempt.PreviousEmail != null)
        {
            attempt.User.Email = attempt.Email;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static string GenerateVerificationCode()
    {
        var randomNumber = new byte[256];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}