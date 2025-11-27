using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.EmailAddresses.Verify;

public class VerifyEmailAddressCommandHandler : ICommandHandler<VerifyEmailAddressCommand>
{
    private readonly IEmailService _service;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailAddressCommandHandler(
        IEmailService service,
        IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(VerifyEmailAddressCommand request, CancellationToken cancellationToken)
    {
        var result = await _service.ConfirmRequestAsync(request.VerificationCode);

        if (result.IsFailure) return result;

        await _unitOfWork.SaveChangesAsync();

        return result;
    }
}