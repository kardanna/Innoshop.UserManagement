using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Deactivate;

public class DeactivateUserCommand : ICommand
{
    public Guid UserId { get; init; }
    public Guid RequesterId { get; init; }
    
    public DeactivateUserCommand(
        Guid userId,
        Guid requesterId)
    {
        UserId = userId;
        RequesterId = requesterId;
    }
}