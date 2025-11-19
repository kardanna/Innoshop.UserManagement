using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Deactivate;

public class DeactivateUserCommand : ICommand
{
    public Guid SubjectId { get; init; }
    public Guid RequesterId { get; init; }
    
    public DeactivateUserCommand(
        Guid subjectId,
        Guid requesterId)
    {
        SubjectId = subjectId;
        RequesterId = requesterId;
    }
}