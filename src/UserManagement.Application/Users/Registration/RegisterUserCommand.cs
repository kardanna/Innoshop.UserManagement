using UserManagement.Application.Messaging;

namespace UserManagement.Application.Users.Registration;

public class RegisterUserCommand : ICommand<Guid>
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; init; }
    public string Password { get; set; }

    public RegisterUserCommand(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string email,
        string password)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;
        Password = password;
    }
}