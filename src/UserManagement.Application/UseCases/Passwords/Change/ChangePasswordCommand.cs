using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Passwords.Change;

public class ChangePasswordCommand : ICommand
{
    public Guid UserId { get; init; }
    public string OldPassword { get; init; }
    public string NewPassword { get; init; }

    public ChangePasswordCommand(
        Guid userId,
        string oldPassword,
        string newPassword)
    {
        UserId = userId;
        OldPassword = oldPassword;
        NewPassword = newPassword;
    }
}