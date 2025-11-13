using FluentValidation;

namespace UserManagement.Application.Users.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(c => c.VerificationCode)
            .NotEmpty();
    }
}