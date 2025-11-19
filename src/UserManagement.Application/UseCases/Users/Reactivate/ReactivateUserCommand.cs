using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Reactivate;

public class ReactivateUserCommand : ICommand
{
    public Guid UserId { get; set; }
    public Guid RequesterId { get; set; }
    
    public ReactivateUserCommand(
        Guid userId,
        Guid requesterId)
    {
        UserId = userId;
        RequesterId = requesterId;
    }
}