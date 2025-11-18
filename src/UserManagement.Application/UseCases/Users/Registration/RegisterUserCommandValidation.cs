using FluentValidation;

namespace UserManagement.Application.UseCases.Users.Registration;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(c => c.LastName)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(c => c.DateOfBirth)
            .GreaterThanOrEqualTo(
                DateOnly.FromDateTime(
                    DateTime.UtcNow
                    .AddYears(-100)))
            .LessThanOrEqualTo(
                DateOnly.FromDateTime(
                    DateTime.UtcNow));

        RuleFor(c => c.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();

        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
    }
}