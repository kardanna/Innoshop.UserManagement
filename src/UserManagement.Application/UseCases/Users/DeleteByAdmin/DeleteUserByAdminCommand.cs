using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.DeleteByAdmin;

public class DeleteUserByAdminCommand : ICommand
{
    public Guid UserId { get; init; }
    public Guid RequesterId { get; init; }

    public DeleteUserByAdminCommand(
        Guid userId,
        Guid requesterId
    )
    {
        UserId = userId;
        RequesterId = requesterId;
    }
}