using FluentValidation;

namespace UserManagement.Application.UseCases.Passwords.Restore;

public class RestorePasswordCommandValidator : AbstractValidator<RestorePasswordCommand>
{
    public RestorePasswordCommandValidator()
    {
        RuleFor(c => c.RestoreCode)
            .NotEmpty();
        
        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
    }
}