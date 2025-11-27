using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.LogoutEverywhere;

public class LogoutUserEverywhereCommandHandler : ICommandHandler<LogoutUserEverywhereCommand>
{
    private readonly ITokenProvider _provider;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutUserEverywhereCommandHandler(
        ITokenProvider provider,
        IUnitOfWork unitOfWokr)
    {
        _provider = provider;
        _unitOfWork = unitOfWokr;
    }

    public async Task<Result> Handle(LogoutUserEverywhereCommand request, CancellationToken cancellationToken)
    {
        var result = await _provider.RevokeAllTokensAsync(request.UserId);

        if (result.IsFailure) return result;

        await _unitOfWork.SaveChangesAsync();

        return result;
    }
}