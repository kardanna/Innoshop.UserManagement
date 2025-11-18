namespace UserManagement.Presentation.DTOs;

public record LoginUserRequest(
    string Email,
    string Password,
    string DeviceFingerprint
);