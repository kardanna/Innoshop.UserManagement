namespace UserManagement.Domain.Shared;

public class Result
{
    public static Result Failure(Error error)
        => new(false, error);
    public static Result Success()
        => new(true, Error.None);  

    public static Result<T> Failure<T>(Error error) =>
        new(default, false, error);
    public static Result<T> Success<T>(T value) =>
        new(value, true, Error.None);

    public static Result<T> Create<T>(T? value) =>
        value is not null ? Success(value) : Failure<T>(Error.NullValue);

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    protected internal Result(bool isSuccess, Error error)
    {
        if ((isSuccess && error != Error.None) ||
            (!isSuccess && error == Error.None))
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static implicit operator Result(Error error) => Failure(error);
}

public class Result<T> : Result
{
    private readonly T? _value;
    public T Value =>
        IsSuccess ? _value! : throw new InvalidOperationException("Can not access value of a failed result");

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static implicit operator Result<T>(T? value) => Create(value);
    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}