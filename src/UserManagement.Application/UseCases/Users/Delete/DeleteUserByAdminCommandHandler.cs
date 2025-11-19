using Microsoft.Extensions.Logging;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Delete;

public class DeleteUserByAdminCommandHandler : ICommandHandler<DeleteUserByAdminCommand>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IInnoshopNotifier _innoshopNotifier;
    private readonly ILogger<DeleteUserByAdminCommandHandler> _logger;

    public DeleteUserByAdminCommandHandler(
        IUserService userService,
        IEmailService emailService,
        ITokenProvider tokenProvider,
        IInnoshopNotifier innoshopNotifier,
        ILogger<DeleteUserByAdminCommandHandler> logger)
    {
        _userService = userService;
        _emailService = emailService;
        _tokenProvider = tokenProvider;
        _innoshopNotifier = innoshopNotifier;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUserByAdminCommand request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("User with ID '{RequesterId}' requested deletion of user with ID {UserId}", request.RequesterId, request.UserId);
        
        var result = await _userService.DeleteByAdminAsync(request.UserId);

        if (result.IsFailure) return result;

        await _emailService.ClearUserRecordsAsync(request.UserId);

        await _tokenProvider.RevokeAllTokensAsync(request.UserId);

        await _innoshopNotifier.SendUserDeletedNotificationAsync(new() { UserId = request.UserId, DeletedAtUtc = DateTime.UtcNow });

        _logger.LogWarning("User with ID '{UserId}' successfully deleted by user with ID {RequesterId}", request.UserId, request.RequesterId);

        return Result.Success();
    }
}