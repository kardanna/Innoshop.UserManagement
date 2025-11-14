namespace UserManagement.Application.Users.Get;

public record GetUserResponse(
    Guid id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Email,
    IEnumerable<string> Roles,
    bool IsDeactivated
);