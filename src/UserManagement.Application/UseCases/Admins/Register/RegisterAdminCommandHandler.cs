using Innoshop.Contracts.UserManagement.UserRoles;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Admins.Register;

public class RegisterAdminCommandHandler : ICommandHandler<RegisterAdminCommand, GetUserResponse>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterAdminCommandHandler> _logger;

    public RegisterAdminCommandHandler(
        IUserService userService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterAdminCommandHandler> logger)
    {
        _userService = userService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<GetUserResponse>> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Registering new admin user by request of '{UserId}' administrator", request.RequesterId);

        var context = new RegistrationContext(request, Role.Administrator);

        var user = await _userService.RegisterAsync(context);

        if (user.IsFailure) return user.Error;

        var sendEmailResult = await _emailService.VerifyAccountAsync(user.Value);

        if (sendEmailResult.IsFailure) return sendEmailResult.Error;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("New administrator '{UserId}' registered successfully. Email verification pending.", user.Value.Id);

        var response = new GetUserResponse(
            user: user.Value,
            isDeactivated: await _userService.IsDeacivated(user.Value.Id)
        );

        return response;
    }
}