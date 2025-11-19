namespace UserManagement.Application.Options;

public class EmailOptions
{
    public int VerificationCodeLifetimeInHours { get; init; }
    public int UserCanChangeEmailOnceInHowManyHours { get; init; }
}