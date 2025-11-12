using UserManagement.Application.Users.Registration;

namespace UserManagement.Application.Contexts;

public class RegistrationContext
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; init; }
    public string Password { get; set; }

    public RegistrationContext(RegisterUserCommand command)
    {
        FirstName = command.FirstName;
        LastName = command.LastName;
        DateOfBirth = command.DateOfBirth;
        Email = command.Email;
        Password = command.Password;
    }
}