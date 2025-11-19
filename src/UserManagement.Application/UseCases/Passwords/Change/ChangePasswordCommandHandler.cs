using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Passwords.Change;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserService _userService;
    private readonly ITokenProvider _tokenProvider;

    public ChangePasswordCommandHandler(
        IUserService userService,
        ITokenProvider tokenProvider)
    {
        _userService = userService;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var context = new ChangePasswordContext(request);

        var response = await _userService.ChangePasswordAsync(context);

        await _tokenProvider.RevokeAllTokensAsync(request.UserId);

        return response;
    }
}