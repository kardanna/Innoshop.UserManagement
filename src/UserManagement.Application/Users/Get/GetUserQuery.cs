using UserManagement.Application.Messaging;

namespace UserManagement.Application.Users.Get;

public class GetUserQuery : IQuery<GetUserResponse>
{
    public Guid Id { get; init; }
    public Guid? RequesterId { get; init; }

    public GetUserQuery(Guid id, Guid? requesterId = null)
    {
        Id = id;
        RequesterId = requesterId;
    }
}