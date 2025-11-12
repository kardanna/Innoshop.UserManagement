using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Users.EmailConfirmation;

public class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand>
{
    private readonly IEmailVerificationService _service;

    public VerifyEmailCommandHandler(IEmailVerificationService service)
    {
        _service = service;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        await _service.VerifyFromRequest(request.VerificationCode);
        return Result.Success();
    }
}