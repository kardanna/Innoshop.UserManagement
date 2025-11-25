using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserManagement.Infrastructure.Messaging.Abstractions;
using RabbitMQ.Client;

namespace UserManagement.Infrastructure.Messaging;

public class RabbitMQConfigurator : IRabbitMQConfigurator, IHostedService
{
    private readonly IRabbitMQConnectionProvider _connectionProvider;
    private readonly ILogger<RabbitMQConfigurator> _logger;
    private readonly TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public RabbitMQConfigurator(
        IRabbitMQConnectionProvider connectionProvider,
        ILogger<RabbitMQConfigurator> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public Task Configure(CancellationToken cancellationToken)
    {
        return _tcs.Task.WaitAsync(cancellationToken);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuring RabbitMQ exchange...");
        
        _logger.LogInformation("Fetching RabbitMQ connection...");
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        _logger.LogInformation("Successfully fetched RabbitMQ connection.");

        _logger.LogInformation("Creating RabbitMQ channel...");
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Successfully created RabbitMQ channel.");

        var exchangeName = Innoshop.Contracts.UserManagement.UserEvents.Exchange.Name;
       
        _logger.LogInformation("Declaring '{ExchangeName}' exchange...", exchangeName);
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            cancellationToken: cancellationToken
        );
        _logger.LogInformation("Successfully declared '{ExchangeName}' exchange.", exchangeName);

        await channel.CloseAsync(cancellationToken);

        _tcs.TrySetResult();

        _logger.LogInformation("Successfully configured RabbitMQ exchange...");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}