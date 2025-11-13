using UserManagement.Domain.Shared;

namespace UserManagement.Application.Models;

public class PolicyResult
{
    public static PolicyResult Success { get; } = new(true, Error.None);
    public static PolicyResult Failure(Error error) => new(false, error);
    
    public bool IsAllowed { get; }
    public bool IsDenied => !IsAllowed;
    public Error Error { get; }

    private PolicyResult(bool isAllowed, Error error)
    {
        if ((isAllowed && error != Error.None) ||
            (!isAllowed && error == Error.None))
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsAllowed = isAllowed;
        Error = error;
    }

    public static implicit operator PolicyResult(Error error)
        => Failure(error);

    public static implicit operator Result(PolicyResult result)
        => result.IsAllowed ? Result.Success() : Result.Failure(result.Error);
}