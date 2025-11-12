using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Persistence;

public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RoleClaim> RoleClaims { get; set; }

    public DbSet<LoginAttempt> LoginAttempts { get; set; }
    public DbSet<EmailVerificationAttempt> EmailVerificationAttempts { get; set; }
    public DbSet<PasswordRestoreAttempt> PasswordRestoreAttempts { get; set; }

    public DbSet<TokenRecord> TokenRecords { get; set; }
    public DbSet<SigningKeyRecord> SigningKeys { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(UserManagement.Infrastructure.AssemblyReference.Assembly);
    }
}