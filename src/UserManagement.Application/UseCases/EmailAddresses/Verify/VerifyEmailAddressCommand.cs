using UserManagement.Application.Messaging;

namespace UserManagement.Application.UseCases.EmailAddresses.Verify;

public class VerifyEmailAddressCommand : ICommand
{
    public string VerificationCode { get; init; }

    public VerifyEmailAddressCommand(string code)
    {
        VerificationCode = code;
    }
}