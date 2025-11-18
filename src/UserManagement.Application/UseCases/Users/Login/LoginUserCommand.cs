using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.Users.Login;

public class LoginUserCommand : ICommand<LoginUserResponse>
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string DeviceFingerprint { get; init; }

    public LoginUserCommand(
        string email,
        string password,
        string deviceFingerprint)
    {
        Email = email;
        Password = password;
        DeviceFingerprint = deviceFingerprint;
    }
}