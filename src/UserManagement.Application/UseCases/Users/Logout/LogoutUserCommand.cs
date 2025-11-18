using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Logout;

public class LogoutUserCommand : ICommand
{
    public Guid TokenId { get; init; }    
    public Guid? RequesterId { get; init; }

    public LogoutUserCommand(
        Guid tokenId,
        Guid? requesterId = null
    )
    {
        TokenId = tokenId;
        RequesterId = requesterId;
    }
}