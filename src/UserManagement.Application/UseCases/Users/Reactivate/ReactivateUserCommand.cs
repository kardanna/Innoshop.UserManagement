using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Reactivate;

public class ReactivateUserCommand : ICommand
{
    public Guid UserId { get; set; }
    
    public ReactivateUserCommand(Guid userId)
    {
        UserId = userId;
    }
}