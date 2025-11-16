using UserManagement.Application.Messaging;

namespace UserManagement.Application.Users.Logout;

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