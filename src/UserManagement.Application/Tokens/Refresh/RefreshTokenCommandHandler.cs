using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.Users.Login;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Tokens.Refresh;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginUserResponse>
{
    private readonly ITokenProvider _tokenService;

    public RefreshTokenCommandHandler(ITokenProvider tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<Result<LoginUserResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _tokenService.GenerateFromRefreshToken(request.RefreshToken);
    }
}