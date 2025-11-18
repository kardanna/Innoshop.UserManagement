using FluentValidation;

namespace UserManagement.Application.UseCases.Users.Delete;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(c => c.Password)
            .NotEmpty()
            .MaximumLength(256);
    }
}