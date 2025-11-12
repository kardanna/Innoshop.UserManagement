using UserManagement.Application.Messaging;
using UserManagement.Application.Users.Login;

namespace UserManagement.Application.Tokens.Refresh;

public class RefreshTokenCommand : ICommand<LoginUserResponse>
{
    public string RefreshToken { get; init; }

    public RefreshTokenCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}