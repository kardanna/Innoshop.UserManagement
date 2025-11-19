using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Persistence.Configuration;

public class UserDeactivationConfiguration : IEntityTypeConfiguration<UserDeactivation>
{
    public void Configure(EntityTypeBuilder<UserDeactivation> builder)
    {
        builder.ToTable("UserDeactivation", "Identity");
        
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DeactivationRequester)
            .WithMany()
            .HasForeignKey(e => e.DeactivationRequesterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.ReactivationRequester)
            .WithMany()
            .HasForeignKey(e => e.ReactivationRequesterId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);        
    }
}