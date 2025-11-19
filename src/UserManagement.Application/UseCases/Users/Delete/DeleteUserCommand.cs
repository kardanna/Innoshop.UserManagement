using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Delete;

public class DeleteUserCommand : ICommand
{
    public Guid SubjectId { get; init; }
    public string? Password { get; init; }
    public Guid RequesterId { get; init; }

    public DeleteUserCommand(
        Guid subjectId,
        string? password,
        Guid requesterId
    )
    {
        SubjectId = subjectId;
        Password = password;
        RequesterId = requesterId;
    }
}