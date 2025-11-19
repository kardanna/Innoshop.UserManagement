using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Passwords.Change;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserService _userService;

    public ChangePasswordCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var context = new ChangePasswordContext(request);

        var response = await _userService.ChangePasswordAsync(context);

        return response;
    }
}