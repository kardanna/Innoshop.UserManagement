namespace UserManagement.Domain.Entities;

public class RoleClaim
{
    public Guid Id { get; set; }
    //public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
    public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}