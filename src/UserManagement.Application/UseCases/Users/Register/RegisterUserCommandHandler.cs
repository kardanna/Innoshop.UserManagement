using Innoshop.Contracts.UserManagement.UserRoles;
using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Register;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, GetUserResponse>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserService userService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var context = new RegistrationContext(request, Role.Customer);

        var user = await _userService.RegisterAsync(context);

        if (user.IsFailure) return user.Error;

        var sendEmailResult = await _emailService.VerifyAccountAsync(user.Value);

        if (sendEmailResult.IsFailure) return sendEmailResult.Error;

        await _unitOfWork.SaveChangesAsync();

        var response = new GetUserResponse(
            user: user.Value,
            isDeactivated: false
        );

        return response;
    }
}