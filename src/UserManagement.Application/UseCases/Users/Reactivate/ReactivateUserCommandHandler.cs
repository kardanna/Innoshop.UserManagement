using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Reactivate;

public class ReactivateUserCommandHandler : ICommandHandler<ReactivateUserCommand>
{
    private readonly IUserService _userService;

    public ReactivateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _userService.ReactivateAsync(request.UserId, request.RequesterId);

        return response;
    }
}