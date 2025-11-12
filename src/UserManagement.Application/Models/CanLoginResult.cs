using UserManagement.Domain.Shared;

namespace UserManagement.Application.Models;

public class CanLoginResult
{
    public static CanLoginResult Success { get; } = new(true, Error.None);
    public static CanLoginResult Failure(Error error) => new(false, error);
    
    public bool IsAllowed { get; }
    public Error Error { get; }

    private CanLoginResult(bool isAllowed, Error error)
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