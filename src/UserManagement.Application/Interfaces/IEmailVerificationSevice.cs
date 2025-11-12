using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IEmailVerificationService
{
    Task SendRequestToVerifyCurrentEmailAsync(User user);
    Task SendRequestToVerifyNewEmailAsync(User user, string newEmail);
    Task VerifyFromRequest(string verificationCode);
}