using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Configuration;

public class SigningKeyRecordConfiguration : IEntityTypeConfiguration<SigningKeyRecord>
{
    public void Configure(EntityTypeBuilder<SigningKeyRecord> builder)
    {
        builder.ToTable("RsaKey", "SigningKeys");

        builder.HasKey(sk => sk.Id);
    }
}