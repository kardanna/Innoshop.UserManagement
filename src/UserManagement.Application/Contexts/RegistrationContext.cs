using UserManagement.Application.UseCases.Users.Register;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Contexts;

public class RegistrationContext
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public Role[] Roles { get; init; }

    public RegistrationContext(RegisterUserCommand command, Role role, params Role[] additionalRoles)
    {
        FirstName = command.FirstName;
        LastName = command.LastName;
        DateOfBirth = command.DateOfBirth;
        Email = command.Email;
        Password = command.Password;
        Roles = [role, .. additionalRoles];
    }
}