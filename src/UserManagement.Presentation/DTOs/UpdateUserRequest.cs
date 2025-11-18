namespace UserManagement.Presentation.DTOs;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth
);