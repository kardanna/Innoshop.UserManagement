using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.EmailAddresses.Change;

public class ChangeEmailAddressCommandHandler : ICommandHandler<ChangeEmailAddressCommand>
{
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeEmailAddressCommandHandler(
        IEmailService emailService,
        IUserService userService,
        IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeEmailAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetAsync(request.UserId);

        if (user.IsFailure) return Result.Failure(user.Error);

        var context = new EmailChangeContext(user, request.NewEmail);

        var emailChangeResult = await _emailService.ChangeEmailAsync(context);

        if (emailChangeResult.IsFailure) return emailChangeResult;

        await _unitOfWork.SaveChangesAsync();

        return emailChangeResult;
    }
}