using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Passwords.Restore;

public class SendPasswordRestoreCodeCommandHandler : ICommandHandler<SendPasswordRestoreCodeCommand>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public SendPasswordRestoreCodeCommandHandler(
        IUserService userService,
        IEmailService emailService)
    {
        _userService = userService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(SendPasswordRestoreCodeCommand request, CancellationToken cancellationToken)
    {
        var attemptCode = await _userService.InitiatePasswordRestorationAsync(request.Email);

        if (attemptCode.IsFailure) return attemptCode;

        var response = await _emailService.SendPasswordResorationCode(request.Email, attemptCode);

        return response;
    }
}