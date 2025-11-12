using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Configuration;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("Role", "Identity");

        builder.HasKey(r => r.Id);

        builder.HasAlternateKey(r => r.Name);
        builder.Property(r => r.Name)
            .HasMaxLength(20);

        builder.HasMany(r => r.Users)
            .WithMany(u => u.Roles);

        builder.HasMany(r => r.Claims)
            .WithMany(c => c.Roles);
    }
}