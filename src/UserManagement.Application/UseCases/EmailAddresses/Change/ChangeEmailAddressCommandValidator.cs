using FluentValidation;

namespace UserManagement.Application.UseCases.EmailAddresses.Change;

public class ChangeEmailAddressCommandValidator : AbstractValidator<ChangeEmailAddressCommand>
{
    public ChangeEmailAddressCommandValidator()
    {
        RuleFor(c => c.NewEmail)
            .NotEmpty()
            .EmailAddress();
    }
}