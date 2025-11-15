namespace UserManagement.Domain.Shared;

public interface IValidationResult
{
    Error[] Errors { get; }
}