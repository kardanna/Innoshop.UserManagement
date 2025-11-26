using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Logout;

public class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
{
    private readonly ITokenProvider _provider;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutUserCommandHandler(
        ITokenProvider provider,
        IUnitOfWork unitOfWork)
    {
        _provider = provider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _provider.RevokeTokenAsync(request.TokenId);

        await _unitOfWork.SaveChangesAsync();

        return response;
    }
}