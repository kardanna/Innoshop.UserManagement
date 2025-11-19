using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Passwords.Restore;

public class RestorePasswordCommandHandler : ICommandHandler<RestorePasswordCommand>
{
    private readonly IUserService _userService;
    private readonly ITokenProvider _tokenProvider;

    public RestorePasswordCommandHandler(
        IUserService userService,
        ITokenProvider tokenProvider)
    {
        _userService = userService;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result> Handle(RestorePasswordCommand request, CancellationToken cancellationToken)
    {
        var response = await _userService.RestorePasswordAsync(
            restoreCode: request.RestoreCode,
            newPassword: request.NewPassword
        );

        if (response.IsFailure) return response;

        return await _tokenProvider.RevokeAllTokensAsync(response.Value);
    }
}