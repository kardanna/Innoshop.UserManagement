using Microsoft.Extensions.DependencyInjection;
using Quartz;
using UserManagement.Infrastructure.BackgroundJobs.Configuration;

namespace UserManagement.Infrastructure.BackgroundJobs;

public static class QurtzDependencyInjection
{
    public static void AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz();

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.ConfigureOptions<RemoveExpiredTokensJobSetup>();
        services.ConfigureOptions<RemoveOldLoginAttemptsJobSetup>();
    }
}