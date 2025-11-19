using UserManagement.Application.UseCases.Users.Login;

namespace UserManagement.Application.Contexts;

public class LoginUserContext
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string DeviceFingerprint { get; init; }

    public LoginUserContext(LoginUserCommand command)
    {
        Email = command.Email;
        Password = command.Password;
        DeviceFingerprint = command.DeviceFingerprint;
    }
}