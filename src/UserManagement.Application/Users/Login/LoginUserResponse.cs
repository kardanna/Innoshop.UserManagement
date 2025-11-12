namespace UserManagement.Application.Users.Login;

public record LoginUserResponse(
    string AccessToken,
    DateTime AccessTokenExpirationDate,
    string RefreshToken
);