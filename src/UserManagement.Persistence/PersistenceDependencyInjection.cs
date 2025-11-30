using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Repositories;
using UserManagement.Infrastructure.Authentication.Repositories;
using UserManagement.Persistence.Repositories;

namespace UserManagement.Persistence;

public static class PersistenceDependencyInjection
{
    public static void AddUserManagementPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ISigningKeyRecordRepository, SigningKeyRecordsRepository>();
        services.AddScoped<ITokenRecordRepository, TokenRecordRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        services.AddScoped<IEmailVerificationAttemptRepository, EmailVerificationAttemptRepository>();
        services.AddScoped<IUserDeactivationRepository, UserDeactivationRepository>();
        services.AddScoped<IPasswordRestoreAttemptRepository, PasswordRestoreAttemptRepository>();
    }
}