using FluentValidation;

namespace UserManagement.Application.UseCases.Passwords.Restore;

public class SendPasswordRestoreCodeCommandValidator : AbstractValidator<SendPasswordRestoreCodeCommand>
{
    public SendPasswordRestoreCodeCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();
    }
}