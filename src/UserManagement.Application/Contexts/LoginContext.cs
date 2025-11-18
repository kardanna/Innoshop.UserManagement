using UserManagement.Application.UseCases.Users.Login;

namespace UserManagement.Application.Contexts;

public class LoginContext
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string DeviceFingerprint { get; init; }

    public LoginContext(LoginUserCommand command)
    {
        Email = command.Email;
        Password = command.Password;
        DeviceFingerprint = command.DeviceFingerprint;
    }
}