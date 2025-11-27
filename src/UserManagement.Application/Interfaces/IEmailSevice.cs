using UserManagement.Application.Contexts;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Interfaces;

public interface IEmailService
{
    Task<Result> VerifyAccountAsync(User user);
    Task<Result> ChangeEmailAsync(EmailChangeContext context);
    Task<Result> ConfirmRequestAsync(string verificationCode);
    Task<Result> SendPasswordRestoreCode(string email, string code);
    Task ClearUserRecordsAsync(Guid userId);
}