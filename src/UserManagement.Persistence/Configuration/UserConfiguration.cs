using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Configuration;

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
            .WithMany()
            .UsingEntity<UserRole>();

        builder.Property(u => u.IsDeactivated)
            .HasDefaultValue(false);

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        builder.HasData( 
            [
                new User()
                { 
                    Id = Guid.Parse("30fc2d9e-3bb0-4bdc-d15b-08de2383d454"),
                    FirstName = "Admin",
                    LastName = "Admin",
                    DateOfBirth = DateOnly.Parse("2000-01-01"),
                    Email = "admin@innoshop.by",
                    PasswordHash = "AQAAAAIAAYagAAAAEBZ2EtG4oB80p/B/1tWjr27MgHcqtVLPyaf7a/wnQsC7/rzf0J2fVO1jMhrGPy5vQw==", //Admin123
                    IsEmailVerified = true
                },
                
                new User()
                {
                    Id = Guid.Parse("160be924-907f-4d70-d15c-08de2383d454"),
                    FirstName = "Ivan",
                    LastName = "Ivanov",
                    Email = "ivan.ivanov@gmail.com",
                    DateOfBirth = DateOnly.Parse("2000-01-01"),
                    PasswordHash = "AQAAAAIAAYagAAAAEDUID6axCz6cvyUWqrPGPCrA+Mm5w8K+1vSgeMrXoqk+NjrjeiCIS9IevKEbet2QdQ==", //IvanIvanov123
                    IsEmailVerified = true
                }
            ]
        );
    }
}