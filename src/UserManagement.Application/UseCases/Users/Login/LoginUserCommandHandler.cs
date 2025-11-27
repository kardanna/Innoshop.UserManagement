using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Login;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserService _userService;
    private readonly ITokenProvider _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginUserCommandHandler(
        IUserService userService,
        ITokenProvider tokenService,
        IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.LoginAsync(new LoginUserContext(request));

        if (user.IsFailure)
        {
            await _unitOfWork.SaveChangesAsync();
            return user.Error;
        }

        var response = await _tokenService.GenerateFromLoginAsync(user.Value, request.DeviceFingerprint);

        await _unitOfWork.SaveChangesAsync();

        return response;
    }
}