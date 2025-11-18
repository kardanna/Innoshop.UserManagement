using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.EmailAddresses.Verify;

public class VerifyEmailAddressCommandHandler : ICommandHandler<VerifyEmailAddressCommand>
{
    private readonly IEmailService _service;

    public VerifyEmailAddressCommandHandler(IEmailService service)
    {
        _service = service;
    }

    public async Task<Result> Handle(VerifyEmailAddressCommand request, CancellationToken cancellationToken)
    {
        return await _service.ConfirmSednedRequestAsync(request.VerificationCode);
    }
}