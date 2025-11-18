using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Delete;

public class DeleteUserCommand : ICommand
{
    public Guid UserId { get; init; }
    public string Password { get; init; }

    public DeleteUserCommand(
        Guid userId,
        string password
    )
    {
        UserId = userId;
        Password = password;
    }
}