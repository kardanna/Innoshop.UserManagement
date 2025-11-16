using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.Users.Get;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Users.Update;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, GetUserResponse>
{
    private readonly IUserService _userService;

    public UpdateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result<GetUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var context = new UpdateUserContext(request);

        var user = await _userService.UpdateUserAsync(context);

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