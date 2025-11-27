using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Passwords.Restore;

public class SendPasswordRestoreCodeCommandHandler : ICommandHandler<SendPasswordRestoreCodeCommand>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public SendPasswordRestoreCodeCommandHandler(
        IUserService userService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SendPasswordRestoreCodeCommand request, CancellationToken cancellationToken)
    {
        var attemptCode = await _userService.InitiatePasswordRestorationAsync(request.Email);

        if (attemptCode.IsFailure) return attemptCode;

        var sendEmailResult = await _emailService.SendPasswordRestoreCode(request.Email, attemptCode);
        
        if (sendEmailResult.IsFailure) return sendEmailResult;

        await _unitOfWork.SaveChangesAsync();

        return sendEmailResult;
    }
}