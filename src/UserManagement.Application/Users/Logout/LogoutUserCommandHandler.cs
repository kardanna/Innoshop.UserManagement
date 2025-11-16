using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Users.Logout;

public class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
{
    private readonly ITokenProvider _provider;

    public LogoutUserCommandHandler(ITokenProvider provider)
    {
        _provider = provider;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _provider.RevokeTokenAsync(request.TokenId);

        return response;
    }
}