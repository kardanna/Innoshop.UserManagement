using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserManagement.Infrastructure.Messaging.Abstractions;
using UserManagement.Infrastructure.Messaging.Options;
using RabbitMQ.Client;

namespace UserManagement.Infrastructure.Messaging;

public class RabbitMQConnectionProvider : IRabbitMQConnectionProvider, IHostedService
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQConnectionProvider> _logger;
    
    private IConnection? connection;
    private readonly TaskCompletionSource<IConnection> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public RabbitMQConnectionProvider(
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQConnectionProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        return _tcs.Task.WaitAsync(cancellationToken);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _logger.LogInformation($"Establishing a RabbitMQ connection...");
        
        connection = await factory.CreateConnectionAsync(cancellationToken);
        _tcs.TrySetResult(connection);
        
        _logger.LogInformation($"Successfully established RabbitMQ connection.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Closing RabbitMQ connection...");
        if (connection != null) await connection.CloseAsync(cancellationToken);
        _logger.LogInformation($"Successfully closed RabbitMQ connection.");
    }
}