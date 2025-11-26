using UserManagement.Domain.Shared;

namespace UserManagement.Application.Interfaces;

public interface IEmailSender
{
    Task<Result> SendAccountVerificationMessageAsync(string email, string verificationCode, string? verificationUrl);
    Task<Result> SendEmailAddressVerificationMessageAsync(string email, string verificationCode, string? verificationUrl);
    Task<Result> SendPasswordRestorationMessageAsync(string email, string code, string? endpoint);
}