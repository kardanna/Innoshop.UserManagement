using UserManagement.Domain.Entities;

namespace UserManagement.Application.UseCases.Users.Get;

public record GetUserResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public string Email { get; init; }
    public IEnumerable<string> Roles { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsDeactivated { get; init; }

    public GetUserResponse(User user, bool isDeactivated)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        DateOfBirth = user.DateOfBirth;
        Email = user.Email;
        Roles = user.Roles.Select(r => r.Name);
        IsEmailVerified = user.IsEmailVerified;
        IsDeactivated = isDeactivated;
    }
}