namespace UserManagement.API.DTOs;

public record VerifyEmailRequest(
    string VerificationCode
);