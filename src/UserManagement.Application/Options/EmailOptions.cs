namespace UserManagement.Application.Options;

public class EmailOptions
{
    public int VerificationCodeLifetimeInHours { get; set; }
    public int UserCanChangeEmailOnceInHowManyHours { get; set; }
}