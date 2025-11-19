using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Login;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserService _userService;
    private readonly ITokenProvider _tokenService;

    public LoginUserCommandHandler(IUserService userService, ITokenProvider tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var loginContext = new LoginUserContext(request);

        var user = await _userService.LoginAsync(loginContext);

        if (user.IsFailure) return user.Error;

        var response = await _tokenService.GenerateFromLoginAsync(user.Value, request.DeviceFingerprint);

        return response;
    }
}