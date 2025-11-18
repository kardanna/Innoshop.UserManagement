using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Deactivate;

public class DeactivateUserCommand : ICommand
{
    public Guid UserId { get; set; }
    
    public DeactivateUserCommand(Guid userId)
    {
        UserId = userId;
    }
}