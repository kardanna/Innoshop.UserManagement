using UserManagement.Domain.Shared;

namespace UserManagement.Domain.Entities;

public class Role : Enumeration<Role>
{
    public static readonly Role Administrator = new(1, "Administrator");
    public static readonly Role Customer = new(2, "Customer");

    public Role(int id, string name)
        : base(id, name)
    {
    }    

    public virtual ICollection<User> Users { get; set; } = new List<User>();
    
    public static implicit operator string(Role role) => role.Name;
}