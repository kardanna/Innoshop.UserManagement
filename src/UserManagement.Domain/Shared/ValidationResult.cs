namespace UserManagement.Domain.Shared;

public class ValidationResult : Result
{
    public static ValidationResult WithErrors(Error[] errors) => new(errors);

    public Error[] Errors { get; init; }

    private ValidationResult(Error[] errors)
        : base(false, Error.ValidationError)
    {
        Errors = errors;
    }
}

public class ValidationResult<T> : Result<T>
{
    public static ValidationResult<T> WithErrors(Error[] errors) => new(errors);

    public Error[] Errors { get; init; }
    
    private ValidationResult(Error[] errors)
        : base(default, false, Error.ValidationError)
    {
        Errors = errors;
    }
}