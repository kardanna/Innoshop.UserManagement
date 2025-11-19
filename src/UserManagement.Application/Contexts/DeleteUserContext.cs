using UserManagement.Application.UseCases.Users.Delete;

namespace UserManagement.Application.Contexts;

public class DeleteUserContext
{
    public Guid SubjectId { get; init; }
    public string? Password { get; init; }
    public Guid RequesterId { get; init; }

    public DeleteUserContext(DeleteUserCommand command)
    {
        SubjectId = command.SubjectId;
        Password = command.Password;
        RequesterId = command.RequesterId;
    }
}