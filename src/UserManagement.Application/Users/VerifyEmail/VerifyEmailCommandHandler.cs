using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Users.VerifyEmail;

public class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand>
{
    private readonly IEmailService _service;

    public VerifyEmailCommandHandler(IEmailService service)
    {
        _service = service;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        return await _service.ConfirmSednedRequestAsync(request.VerificationCode);
    }
}