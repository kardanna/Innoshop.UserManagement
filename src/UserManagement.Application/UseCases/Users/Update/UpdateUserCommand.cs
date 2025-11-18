using UserManagement.Application.Messaging;
using UserManagement.Application.UseCases.Users.Get;

namespace UserManagement.Application.UseCases.Users.Update;

public class UpdateUserCommand : ICommand<GetUserResponse>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateOnly DateOfBirth { get; init; }

    public UpdateUserCommand(
        Guid userId,
        string firstName,
        string lastName,
        DateOnly dateOfBirth
    )
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }
}