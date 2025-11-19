using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Passwords.Restore;

public class RestorePasswordCommand : ICommand
{
    public string RestoreCode { get; init; }
    public string NewPassword { get; init; }

    public RestorePasswordCommand(
        string restoreCode,
        string newPassword)
    {
        RestoreCode = restoreCode;
        NewPassword = newPassword;
    }
}