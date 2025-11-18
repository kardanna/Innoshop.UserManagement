using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Delete;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IInnoshopNotifier _innoshopNotifier;

    public DeleteUserCommandHandler(
        IUserService userService,
        IEmailService emailService,
        ITokenProvider tokenProvider,
        IInnoshopNotifier innoshopNotifier)
    {
        _userService = userService;
        _emailService = emailService;
        _tokenProvider = tokenProvider;
        _innoshopNotifier = innoshopNotifier;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var context = new DeleteUserContext(request);

        var result = await _userService.DeleteAsync(context);

        if (result.IsFailure) return result;

        await _emailService.ClearUserRecordsAsync(request.UserId);

        await _tokenProvider.RevokeAllTokensAsync(request.UserId);

        await _innoshopNotifier.SendUserDeletedNotificationAsync(new() { UserId = request.UserId, DeletedAtUtc = DateTime.UtcNow });

        return Result.Success();
    }
}