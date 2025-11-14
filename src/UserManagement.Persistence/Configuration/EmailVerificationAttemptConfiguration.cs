using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Configuration;

public class EmailVerificationAttemptConfiguration : IEntityTypeConfiguration<EmailVerificationAttempt>
{
    public void Configure(EntityTypeBuilder<EmailVerificationAttempt> builder)
    {
        builder.ToTable("EmailVerificationAttempt", "Identity");

        builder.HasKey(a => a.Id);

        builder.HasAlternateKey(a => a.VerificationCode);

        builder.Property(a => a.IsSucceeded)
            .HasDefaultValue(false);

        builder.HasOne(a => a.User)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);
    }
}