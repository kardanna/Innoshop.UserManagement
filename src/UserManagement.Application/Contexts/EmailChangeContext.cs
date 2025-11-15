using UserManagement.Domain.Entities;

namespace UserManagement.Application.Contexts;

public record EmailChangeContext(
    User User,
    string NewEmail
);