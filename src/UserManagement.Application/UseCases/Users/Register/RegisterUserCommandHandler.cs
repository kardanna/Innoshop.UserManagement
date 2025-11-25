using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Domain.Entities;
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

        var sendEmailResult = await _emailService.SendRequestToVerifyUserAccountAsync(user.Value);

        if (sendEmailResult.IsFailure) return sendEmailResult.Error;

        await _unitOfWork.SaveChangesAsync();

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

        return response;
    }
}