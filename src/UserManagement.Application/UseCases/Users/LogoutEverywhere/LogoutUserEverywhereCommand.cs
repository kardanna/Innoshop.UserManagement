using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.LogoutEverywhere;

public class LogoutUserEverywhereCommand : ICommand
{
    public Guid UserId { get; init; }    
    public Guid? RequesterId { get; init; }

    public LogoutUserEverywhereCommand(
        Guid userId,
        Guid? requesterId = null
    )
    {
        UserId = userId;
        RequesterId = requesterId;
    }
}