using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Reactivate;

public class ReactivateUserCommandHandler : ICommandHandler<ReactivateUserCommand>
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInnoshopNotifier _innoshopNotifier;

    public ReactivateUserCommandHandler(
        IUserService userService,
        IUnitOfWork unitOfWork,
        IInnoshopNotifier innoshopNotifier)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _innoshopNotifier = innoshopNotifier;
    }

    public async Task<Result> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var reactivationResult = await _userService.ReactivateAsync(request.UserId, request.RequesterId);

        if (reactivationResult.IsFailure) return reactivationResult;

        var notificationResult = await _innoshopNotifier.SendUserReactivatedNotificationAsync(new()
            {
                UserId = request.UserId,
                TimeStamp = DateTime.UtcNow
            }
        );

        if (notificationResult.IsFailure) return notificationResult;

        await _unitOfWork.SaveChangesAsync();

        return notificationResult;
    }
}