using UserManagement.Application.Messaging;

namespace UserManagement.Application.Users.EmailConfirmation;

public class VerifyEmailCommand : ICommand
{
    public string VerificationCode { get; init; }

    public VerifyEmailCommand(string code)
    {
        VerificationCode = code;
    }
}