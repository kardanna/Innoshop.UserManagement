using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.Users.Get;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Users.Registration;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, GetUserResponse>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public RegisterUserCommandHandler(
        IUserService userService,
        IEmailService emailService)
    {
        _userService = userService;
        _emailService = emailService;
    }

    public async Task<Result<GetUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var context = new RegistrationContext(request);

        var user = await _userService.RegisterAsync(context);

        if (user.IsFailure) return user.Error;

        await _emailService.SendRequestToVerifyUserEmailAsync(user.Value);

        var response = new GetUserResponse(
            user.Value.Id,
            user.Value.FirstName,
            user.Value.LastName,
            user.Value.DateOfBirth,
            user.Value.Email,
            user.Value.Roles.Select(r => r.Name),
            user.Value.IsEmailVerified,
            user.Value.IsDeactivated
        );

        return response;
    }
}