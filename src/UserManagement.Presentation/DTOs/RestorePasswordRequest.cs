namespace UserManagement.Presentation.DTOs;

public record RestorePasswordRequest(
    string RestoreCode,
    string NewPassword
);