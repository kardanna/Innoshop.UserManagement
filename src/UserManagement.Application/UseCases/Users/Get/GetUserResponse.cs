namespace UserManagement.Application.UseCases.Users.Get;

public record GetUserResponse(
    Guid id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Email,
    IEnumerable<string> Roles,
    bool IsEmailVerified,
    bool IsDeactivated
);