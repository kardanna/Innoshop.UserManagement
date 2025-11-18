using UserManagement.Application.UseCases.Users.Register;

namespace UserManagement.Application.UseCases.Admins.Register;

public class RegisterAdminCommand : RegisterUserCommand
{
    public Guid RequesterId { get; init; }

    public RegisterAdminCommand(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string email,
        string password,
        Guid requesterId)
        : base(firstName, lastName, dateOfBirth, email, password)
    {
        RequesterId = requesterId;
    }
}