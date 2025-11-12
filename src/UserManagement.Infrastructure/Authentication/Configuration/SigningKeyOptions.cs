namespace UserManagement.Infrastructure.Authentication.Configuration;

public class SigningKeyOptions
{
    public int SingingKeyLifetimeDays { get; init; }
    public int ValidationKeyLifetimeDays { get; init; }
    public int KeySizeBytes { get; init; }
}