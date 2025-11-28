using Microsoft.Extensions.Options;
using Quartz;

namespace UserManagement.Infrastructure.BackgroundJobs.Configuration;

public class RemoveOldLoginAttemptsJobSetup : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(RemoveOldLoginAttemptsJob));
        
        options
            .AddJob<RemoveOldLoginAttemptsJob>(
                jb => jb.WithIdentity(jobKey))
            .AddTrigger(tb => tb
                .ForJob(jobKey)
                .WithCronSchedule("0 0 0 * * ?"));
    }
}