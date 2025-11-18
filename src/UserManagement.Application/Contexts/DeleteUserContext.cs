using UserManagement.Application.UseCases.Users.Delete;

namespace UserManagement.Application.Contexts;

public class DeleteUserContext
{
    public Guid UserId { get; init; }
    public string Password { get; init; }

    public DeleteUserContext(DeleteUserCommand command)
    {
        UserId = command.UserId;
        Password = command.Password;
    }
}