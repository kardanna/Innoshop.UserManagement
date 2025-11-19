using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Passwords.Restore;

public class RestorePasswordCommandHandler : ICommandHandler<RestorePasswordCommand>
{
    private readonly IUserService _userService;

    public RestorePasswordCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result> Handle(RestorePasswordCommand request, CancellationToken cancellationToken)
    {
        return await _userService.RestorePasswordAsync(
            restoreCode: request.RestoreCode,
            newPassword: request.NewPassword
        );
    }
}