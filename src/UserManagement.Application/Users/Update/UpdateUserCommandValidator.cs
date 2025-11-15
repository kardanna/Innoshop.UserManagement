using FluentValidation;

namespace UserManagement.Application.Users.Update;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
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
    }
}