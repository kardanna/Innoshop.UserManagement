using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.EmailAddresses.Change;

public class ChangeEmailAddressCommand : ICommand
{
    public Guid UserId { get; init; }
    public string NewEmail { get; init; }

    public ChangeEmailAddressCommand(Guid userId, string newEmail)
    {
        UserId = userId;
        NewEmail = newEmail;
    }
}