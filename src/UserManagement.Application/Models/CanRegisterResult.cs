using UserManagement.Domain.Shared;

namespace UserManagement.Application.Models;

public class CanRegisterResult
{
    public static CanRegisterResult Success { get; } = new(true, Error.None);
    public static CanRegisterResult Failure(Error error) => new(false, error);
    
    public bool IsAllowed { get; }
    public Error Error { get; }

    private CanRegisterResult(bool isAllowed, Error error)
    {
        if ((isAllowed && error != Error.None) ||
            (!isAllowed && error == Error.None))
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsAllowed = isAllowed;
        Error = error;
    }
}