using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Get;

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, GetUserResponse>
{
    private readonly IUserService _userService;

    public GetUserQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result<GetUserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        //Log requester?

        var user = await _userService.GetUserAsync(request.Id);

        if (user.IsFailure) return user.Error;

        var response = new GetUserResponse(
            user.Value.Id,
            user.Value.FirstName,
            user.Value.LastName,
            user.Value.DateOfBirth,
            user.Value.Email,
            user.Value.Roles.Select(r => r.Name),
            user.Value.IsEmailVerified,
            user.Value.IsDeactivated
        );

        return response;
    }
}