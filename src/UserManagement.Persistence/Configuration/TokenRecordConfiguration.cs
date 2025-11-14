using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Configuration;

public class TokenRecordConfiguration : IEntityTypeConfiguration<TokenRecord>
{
    public void Configure(EntityTypeBuilder<TokenRecord> builder)
    {
        builder.ToTable("TokenRecord", "Authentication");

        builder.HasKey(tr => tr.AccessTokenId);
        builder.Property(tr => tr.AccessTokenId)
            .ValueGeneratedNever();

        builder.HasOne(tr => tr.User)
            .WithMany()
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}