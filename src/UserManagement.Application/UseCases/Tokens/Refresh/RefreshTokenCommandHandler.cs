using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.UseCases.Users.Login;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Tokens.Refresh;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginUserResponse>
{
    private readonly ITokenProvider _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        ITokenProvider tokenService,
        IUnitOfWork unitOfWork)
    {
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginUserResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _tokenService.GenerateFromRefreshTokenAsync(request.RefreshToken);

        if (result.IsFailure) return result;

        await _unitOfWork.SaveChangesAsync();

        return result;
    }
}