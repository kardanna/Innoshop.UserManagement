using UserManagement.Application.Messaging;

namespace UserManagement.Application.Users.ChangeEmail;

public class ChangeEmailCommand : ICommand
{
    public Guid UserId { get; init; }
    public string NewEmail { get; init; }

    public ChangeEmailCommand(Guid userId, string newEmail)
    {
        UserId = userId;
        NewEmail = newEmail;
    }
}