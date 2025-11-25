namespace UserManagement.Application.Options;

public class EmailOptions
{
    public int VerificationCodeLifetimeInHours { get; init; }
    public int UserCanChangeEmailOnceInHowManyHours { get; init; }
    public string AccountVerificationCallbackUrl { get; init; } = null!;
    public string EmailVerificationCallbackUrl { get; init; } = null!;
    public string PasswordRestoreCallbackUrl { get; init; } = null!;
}