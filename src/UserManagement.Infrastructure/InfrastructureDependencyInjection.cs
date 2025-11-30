using Microsoft.Extensions.DependencyInjection;
using UserManagement.Infrastructure.Authentication;
using UserManagement.Infrastructure.BackgroundJobs;
using UserManagement.Infrastructure.EmailSender;
using UserManagement.Infrastructure.Messaging;

namespace UserManagement.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static void AddUserManagementInfrastructure(this IServiceCollection services)
    {
        services.AddEmailSender();

        services.AddBackgroundJobs();

        services.AddMessaging();

        services.AddAuth();
    } 
}