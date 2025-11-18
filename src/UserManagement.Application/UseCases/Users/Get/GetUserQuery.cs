using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Get;

public class GetUserQuery : IQuery<GetUserResponse>
{
    public Guid UserId { get; init; }
    public Guid? RequesterId { get; init; }

    public GetUserQuery(Guid userId, Guid? requesterId = null)
    {
        UserId = userId;
        RequesterId = requesterId;
    }
}