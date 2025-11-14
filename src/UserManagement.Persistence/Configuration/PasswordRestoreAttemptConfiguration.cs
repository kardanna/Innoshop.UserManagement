using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Configuration;

public class PasswordRestoreAttemptConfiguration : IEntityTypeConfiguration<PasswordRestoreAttempt>
{
    public void Configure(EntityTypeBuilder<PasswordRestoreAttempt> builder)
    {
        builder.ToTable("PasswordRestoreAttempt", "Identity");

        builder.HasKey(a => a.Id);

        builder.HasAlternateKey(a => a.AttemptCode);

        builder.HasOne(a => a.User)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);
    }
}