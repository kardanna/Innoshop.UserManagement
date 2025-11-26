namespace UserManagement.Application.Interfaces;

public interface IUrlProvider
{
    string? GetUrlForEmailVerificationEndpoint(string verificationCode);
    string? GetUrlForPasswordRestoreEndpoint();
}