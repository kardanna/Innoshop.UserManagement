using FluentValidation;

namespace UserManagement.Application.UseCases.EmailAddresses.Verify;

public class VerifyEmailAddressCommandValidator : AbstractValidator<VerifyEmailAddressCommand>
{
    public VerifyEmailAddressCommandValidator()
    {
        RuleFor(c => c.VerificationCode)
            .NotEmpty();
    }
}