using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Configuration;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole", "Identity");

        builder.HasKey(e => new { e.UserId, e.RoleId });

        builder.HasData(
            [
                new UserRole()
                {
                    UserId = Guid.Parse("30fc2d9e-3bb0-4bdc-d15b-08de2383d454"),
                    RoleId = 1
                },
                
                new UserRole()
                {
                    UserId = Guid.Parse("160be924-907f-4d70-d15c-08de2383d454"),
                    RoleId = 2
                }
            ]
        );
    }
}