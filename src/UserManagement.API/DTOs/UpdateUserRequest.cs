namespace UserManagement.API.DTOs;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth
);