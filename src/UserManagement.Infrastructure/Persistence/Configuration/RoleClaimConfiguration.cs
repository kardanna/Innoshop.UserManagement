using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Configuration;

public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable("Claim", "Identity");

        /*builder.HasKey(c => c.Id);

        builder.HasAlternateKey(c => c.Name);
        builder.Property(c => c.Name)
            .HasMaxLength(40);*/

        builder.HasKey(c => new { c.Type, c.Value });

        builder.Property(c => c.Type)
            .HasMaxLength(15);

        builder.Property(c => c.Value)
            .HasMaxLength(40);

        builder.HasMany(c => c.Roles)
            .WithMany(r => r.Claims);
    }
}