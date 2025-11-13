using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", "Identity");

        builder.HasKey(u => u.Id);     

        builder.Property(u => u.FirstName)
            .HasMaxLength(20);

        builder.Property(u => u.LastName)
            .HasMaxLength(30);
        
        //builder.HasAlternateKey(u => u.Email);
        builder.Property(u => u.Email)
            .HasMaxLength(256);

        builder.Property(u => u.IsEmailVerified)
            .HasDefaultValue(false);

        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users);

        builder.Property(u => u.IsDeactivated)
            .HasDefaultValue(false);

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        /*builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(u => u.LastModifiedAt)
            .HasComputedColumnSql("GETUTCDATE()", true)
            .ValueGeneratedOnAddOrUpdate();*/

        builder.HasData(
            new User()
            {
                Id = Guid.Parse("2f6ba6b8-e14d-4b05-942d-e2c1344ce708"),
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan.ivanov@gmail.com",
                PasswordHash = "123456",
                IsEmailVerified = true
            }
        );
    }
}