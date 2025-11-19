namespace UserManagement.Presentation.DTOs;

public record ChangePasswordRequest(
    string OldPassword,
    string NewPassword
);