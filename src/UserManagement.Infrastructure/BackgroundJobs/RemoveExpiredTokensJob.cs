using Microsoft.Extensions.Logging;
using Quartz;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Options;
using UserManagement.Infrastructure.Authentication.Repositories;

namespace UserManagement.Infrastructure.BackgroundJobs;

public class RemoveExpiredTokensJob : IJob
{
    private readonly ITokenRecordRepository _tokenRecordRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RemoveExpiredTokensJob> _logger;

    public RemoveExpiredTokensJob(
        ITokenRecordRepository tokenRecordRepository,
        JwtOptions jwtOptions,
        ILogger<RemoveExpiredTokensJob> logger)
    {
        _tokenRecordRepository = tokenRecordRepository;
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Deleting expired tokens from the database...");
        
        int removedTokens = 0;
        
        try
        {
            removedTokens = await _tokenRecordRepository.RemoveExpiredRecordsAsync(_jwtOptions.RefreshTokenLifetimeMinutes);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to delete expired tokens from the database.");
        }

        _logger.LogInformation("Successfully deleted expired tokens from the database. Removed a total of {RemovedTokensCount} token records", removedTokens);
    }
}