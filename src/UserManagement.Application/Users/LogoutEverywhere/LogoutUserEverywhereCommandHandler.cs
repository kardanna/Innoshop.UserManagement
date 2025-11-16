using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Users.LogoutEverywhere;

public class LogoutUserEverywhereCommandHandler : ICommandHandler<LogoutUserEverywhereCommand>
{
    private readonly ITokenProvider _provider;

    public LogoutUserEverywhereCommandHandler(ITokenProvider provider)
    {
        _provider = provider;
    }

    public async Task<Result> Handle(LogoutUserEverywhereCommand request, CancellationToken cancellationToken)
    {
        //log requester???
        var response = await _provider.RevokeAllTokensAsync(request.UserId);

        return response;
    }
}