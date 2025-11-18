using UserManagement.Application.Contexts;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Interfaces;

public interface IEmailService
{
    Task SendRequestToVerifyUserEmailAsync(User user);
    Task<Result> SendRequestToChangeUserEmailAsync(EmailChangeContext context);
    Task<Result> ConfirmSednedRequestAsync(string verificationCode);
    Task ClearUserRecordsAsync(Guid userId);
}