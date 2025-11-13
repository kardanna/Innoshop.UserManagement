using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Users.ChangeEmail;

public class ChangeEmailCommandHandler : ICommandHandler<ChangeEmailCommand>
{
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;

    public ChangeEmailCommandHandler(
        IEmailService emailService,
        IUserService userService)
    {
        _emailService = emailService;
        _userService = userService;
    }

    public async Task<Result> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserAsync(request.UserId);

        if (user.IsFailure) return Result.Failure(user.Error);

        var context = new EmailChangeContext(user, request.NewEmail);

        var response = await _emailService.SendRequestToChangeUserEmailAsync(context);

        return response;
    }
}