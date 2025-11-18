namespace UserManagement.Presentation.DTOs;

public record VerifyEmailRequest(
    string VerificationCode
);