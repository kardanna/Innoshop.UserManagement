using FluentValidation;

namespace UserManagement.Application.Users.ChangeEmail;

public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    public ChangeEmailCommandValidator()
    {
        RuleFor(c => c.NewEmail)
            .NotEmpty()
            .EmailAddress();
    }
}