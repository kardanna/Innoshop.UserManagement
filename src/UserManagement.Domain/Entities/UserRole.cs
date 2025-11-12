namespace UserManagement.Domain.Entities;

public class UserRole
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<RoleClaim> Claims { get; set; } = new List<RoleClaim>();
}