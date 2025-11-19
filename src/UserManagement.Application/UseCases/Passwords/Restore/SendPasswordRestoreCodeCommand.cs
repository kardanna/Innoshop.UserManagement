using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Passwords.Restore;

public class SendPasswordRestoreCodeCommand : ICommand
{
    public string Email { get; init; }

    public SendPasswordRestoreCodeCommand(string email)
    {
        Email = email;
    }
}