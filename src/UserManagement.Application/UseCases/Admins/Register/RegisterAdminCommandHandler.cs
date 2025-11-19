using Microsoft.Extensions.Logging;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Admins.Register;

public class RegisterAdminCommandHandler : ICommandHandler<RegisterAdminCommand, GetUserResponse>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterAdminCommandHandler> _logger;

    public RegisterAdminCommandHandler(
        IUserService userService,
        IEmailService emailService,
        ILogger<RegisterAdminCommandHandler> logger)
    {
        _userService = userService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<GetUserResponse>> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Registering new admin user by request of '{UserId}' administrator", request.RequesterId);

        var context = new RegistrationContext(request, Role.Administrator);

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
            await _userService.IsUserDeacivated(user.Value.Id)
        );

        _logger.LogWarning("New administrator '{UserId}' registered successfully. Email verification pending.", user.Value.Id);

        return response;
    }
}