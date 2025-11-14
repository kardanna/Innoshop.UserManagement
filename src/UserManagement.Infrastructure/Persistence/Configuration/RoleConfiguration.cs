using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Configuration;

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

        builder.HasMany(r => r.Users)
            .WithMany(u => u.Roles)
            .UsingEntity<UserRole>();

        builder.HasData(Role.GetValues());
    }
}