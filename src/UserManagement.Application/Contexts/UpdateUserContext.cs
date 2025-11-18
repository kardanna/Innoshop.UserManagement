using UserManagement.Application.UseCases.Users.Update;

namespace UserManagement.Application.Contexts;

public class UpdateUserContext
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateOnly DateOfBirth { get; init; }

    public UpdateUserContext(UpdateUserCommand command)
    {
        UserId = command.UserId;
        FirstName = command.FirstName;
        LastName = command.LastName;
        DateOfBirth = command.DateOfBirth;
    }
}