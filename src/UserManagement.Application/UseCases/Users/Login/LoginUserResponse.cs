namespace UserManagement.Application.UseCases.Users.Login;

public record LoginUserResponse(
    string AccessToken,
    DateTime AccessTokenExpirationDate,
    string RefreshToken
);