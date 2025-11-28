using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using UserManagement.Application.Options;
using UserManagement.Application.Repositories;

namespace UserManagement.Infrastructure.BackgroundJobs;

public class RemoveOldLoginAttemptsJob : IJob
{
    private readonly ILoginAttemptRepository _loginAttemptRepository;
    private readonly LoginOptions _loginOptions;
    private readonly ILogger<RemoveOldLoginAttemptsJob> _logger;

    public RemoveOldLoginAttemptsJob(
        ILoginAttemptRepository loginAttemptRepository,
        IOptions<LoginOptions> loginOptions,
        ILogger<RemoveOldLoginAttemptsJob> logger)
    {
        _loginAttemptRepository = loginAttemptRepository;
        _loginOptions = loginOptions.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Deleting old login attempts from the database...");
        
        int removedRecords = 0;
        
        try
        {
            removedRecords = await _loginAttemptRepository.RemoveOldLoginAttemptsAsync(_loginOptions.LoginAttemptsTimeWindowInMinutes);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old login attempts from the database.");
        }

        _logger.LogInformation("Successfully deleted old login attempts from the database. Removed a total of {RemovedLoginAttemptRecordsCount} login attempt records", removedRecords);
    }
}