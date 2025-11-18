using UserManagement.Domain.Shared;

namespace UserManagement.Domain.Entities;

public class Role : Enumeration<Role>
{
    // Newly added static Roles must me migrated to DB
    public static readonly Role Administrator = new(1, "Administrator");
    public static readonly Role Customer = new(2, "Customer");

    public Role(int id, string name)
        : base(id, name)
    {
    }
    
    public static implicit operator string(Role role) => role.Name;
}