using Microsoft.Extensions.Options;
using Quartz;

namespace UserManagement.Infrastructure.BackgroundJobs.Configuration;

public class RemoveExpiredTokensJobSetup : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(RemoveExpiredTokensJob));
        
        options
            .AddJob<RemoveExpiredTokensJob>(
                jb => jb.WithIdentity(jobKey))
            .AddTrigger(tb => tb
                .ForJob(jobKey)
                .WithCronSchedule("0 0 0 * * ?"));
    }
}