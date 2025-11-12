using FluentValidation;

namespace UserManagement.Application.Users.EmailConfirmation;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(c => c.VerificationCode)
            .NotEmpty();
    }
}