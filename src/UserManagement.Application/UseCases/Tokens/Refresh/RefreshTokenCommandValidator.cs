using FluentValidation;

namespace UserManagement.Application.UseCases.Tokens.Refresh;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(c => c.RefreshToken)
            .NotEmpty();
    }
}