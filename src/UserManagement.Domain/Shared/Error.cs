namespace UserManagement.Domain.Shared;

public class Error
{
    public string Code { get; init; } = null!;
    public string? Description { get; init; }

    public static readonly Error None = new(string.Empty);
    public static readonly Error NullValue = new("Error.NullValue");
    public static readonly Error ValidationError = new("ValidationError", "A validation problem has occurred.");

    public Error(string code, string? description = null)
    {
        Code = code;
        Description = description;
    }

    public static implicit operator Result(Error error) => Result.Failure(error);
}