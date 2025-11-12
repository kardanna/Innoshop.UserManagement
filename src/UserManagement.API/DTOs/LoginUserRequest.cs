namespace UserManagement.API.DTOs;

public record LoginUserRequest(
    string Email,
    string Password,
    string DeviceFingerprint
);