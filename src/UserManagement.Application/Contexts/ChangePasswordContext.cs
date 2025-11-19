using UserManagement.Application.UseCases.Passwords.Change;

namespace UserManagement.Application.Contexts;

public class ChangePasswordContext
{
    public Guid UserId { get; init; }
    public string OldPassword { get; init; }
    public string NewPassword { get; init; }

    public ChangePasswordContext(ChangePasswordCommand command)
    {
        UserId = command.UserId;
        OldPassword = command.OldPassword;
        NewPassword = command.NewPassword;
    }
}