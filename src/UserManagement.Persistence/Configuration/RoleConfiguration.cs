using Innoshop.Contracts.UserManagement.UserRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Persistence.Configuration;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role", "Identity");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.HasAlternateKey(r => r.Name);
        builder.Property(r => r.Name)
            .HasMaxLength(20);

        builder.HasData(Role.GetValues());
    }
}