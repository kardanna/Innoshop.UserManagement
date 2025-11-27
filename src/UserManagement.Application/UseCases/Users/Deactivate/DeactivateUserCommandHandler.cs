using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Deactivate;

public class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInnoshopNotifier _innoshopNotifier;

    public DeactivateUserCommandHandler(
        IUserService userService,
        IUnitOfWork unitOfWork,
        IInnoshopNotifier innoshopNotifier)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _innoshopNotifier = innoshopNotifier;
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var deactivationResult = await _userService.DeactivateAsync(request.SubjectId, request.RequesterId);

        if (deactivationResult.IsFailure) return deactivationResult;

        var notificationResult = await _innoshopNotifier.SendUserDeactivatedNotificationAsync(new()
            {
                UserId = request.SubjectId,
                TimeStamp = DateTime.UtcNow
            }
        );

        if (notificationResult.IsFailure) return notificationResult;

        await _unitOfWork.SaveChangesAsync();

        return notificationResult;
    }
}