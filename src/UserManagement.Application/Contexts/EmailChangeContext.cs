using UserManagement.Domain.Entities;

namespace UserManagement.Application.Contexts;

public class EmailChangeContext
{
    public User User { get; init; }
    public string NewEmail { get; init; }

    public EmailChangeContext(User user, string newEmail)
    {
        User = user;
        NewEmail = newEmail;
    }
}