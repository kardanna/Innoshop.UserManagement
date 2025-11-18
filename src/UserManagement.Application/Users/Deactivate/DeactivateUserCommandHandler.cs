using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Users.Deactivate;

public class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
{
    private readonly IUserService _userService;

    public DeactivateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _userService.DeactivateUserAsync(request.UserId);

        return response;
    }
}