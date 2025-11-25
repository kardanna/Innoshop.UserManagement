using FluentEmail.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.Infrastructure.EmailSender;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
        IFluentEmail fluentEmail,
        ILogger<EmailSender> logger,
        IConfiguration configuration)
    {
        _fluentEmail = fluentEmail;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<Result> SendAccountVerificationMessageAsync(string email, string verificationUrl)
    {
        try
        {
            _logger.LogInformation("Attempting to send account verification message to {EmailAddress}...", email);

            await _fluentEmail
                .To(email)
                .Subject("Innoshop account verification")
                .Body($"To verify your account <a href='{verificationUrl}'>click this link</a> or paste this address into your browser: {verificationUrl}", isHtml: true)
                .SendAsync();
            
            _logger.LogInformation("Successfully sent account verification message to {EmailAddress}.", email);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send account verification message to {EmailAddress}.", email);
            return Result.Failure(DomainErrors.Email.FailedToSend);
        }
    }

    public async Task<Result> SendEmailAddressVerificationMessageAsync(string email, string verificationUrl)
    {
        try
        {
            _logger.LogInformation("Attempting to send email address verification message to {EmailAddress}...", email);

            await _fluentEmail
                .To(email)
                .Subject("Innoshop email verification")
                .Body($"To verify your email <a href='{verificationUrl}'>click this link</a> or paste this address into your browser: {verificationUrl}", isHtml: true)
                .SendAsync();
            
            _logger.LogInformation("Successfully sent email address verification message to {EmailAddress}.", email);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email address verification message to {EmailAddress}.", email);
            return Result.Failure(DomainErrors.Email.FailedToSend);
        }
    }

    public async Task<Result> SendPasswordRestorationMessageAsync(string email, string code)
    {
        try
        {
            _logger.LogInformation("Attempting to send password restoration message to {EmailAddress}...", email);

            await _fluentEmail
                .To(email)
                .Subject("Innoshop password restore")
                .Body($"To restore your password provide 'NewPassword' field and the following 'RestoreCode' in a POST request's body to this endpoint: verificationUrl. Restore code: '{code}'.") //INSERT URL
                .SendAsync();
            
            _logger.LogInformation("Successfully sent password restoration message to {EmailAddress}.", email);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password restoration message to {EmailAddress}.", email);
            return Result.Failure(DomainErrors.Email.FailedToSend);
        }
    }
}