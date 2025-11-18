using UserManagement.Application.Messaging;
using UserManagement.Application.UseCases.Users.Login;

namespace UserManagement.Application.UseCases.Tokens.Refresh;

public class RefreshTokenCommand : ICommand<LoginUserResponse>
{
    public string RefreshToken { get; init; }

    public RefreshTokenCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}