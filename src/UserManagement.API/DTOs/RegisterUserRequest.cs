namespace UserManagement.API.DTOs;

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Email,
    string Password
);