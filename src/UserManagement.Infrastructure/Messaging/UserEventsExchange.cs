using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using UserManagement.Domain.Exceptions;
using UserManagement.Infrastructure.Messaging.Abstractions;

namespace UserManagement.Infrastructure.Messaging;

public class UserEventsExchange : IExchange, IHostedService
{
    public const string EXCHANGE_NAME = Innoshop.Contracts.UserManagement.UserEvents.Exchange.Name;

    private readonly IRabbitMQConnectionProvider _connectionProvider;
    private readonly IRabbitMQConfigurator _configurator;
    private readonly ILogger<UserEventsExchange> _logger;

    private IChannel? channel;
    private readonly BasicProperties properties = new() { Persistent = true };

    public UserEventsExchange(
        IRabbitMQConnectionProvider connectionProvider,
        IRabbitMQConfigurator configurator,
        ILogger<UserEventsExchange> logger)
    {
        _connectionProvider = connectionProvider;
        _configurator = configurator;
        _logger = logger;
    }

    public async Task SendMessage(string topic, object notification)
    {
        if (channel is null) throw new UninitializedExchangeChannelException();

        var json = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(json);
        await channel.BasicPublishAsync(
            exchange: EXCHANGE_NAME,
            routingKey: topic,
            mandatory: true,
            body: body,
            basicProperties: properties
        );
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _configurator.Configure(cancellationToken);

        _logger.LogInformation($"Configuring '{EXCHANGE_NAME}' exchange message sender...");
        _logger.LogInformation("Fetching RabbitMQ connection...");
        
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        
        _logger.LogInformation("Successfully fetched RabbitMQ connection.");
        _logger.LogInformation("Creating RabbitMQ channel...");
        
        channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        _logger.LogInformation("Successfully created RabbitMQ channel.");
        _logger.LogInformation($"Successfully configured '{EXCHANGE_NAME}' exchange message sender.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Closing the channel for sending messages to '{ExchangeName}'...", EXCHANGE_NAME);
        if (channel is not null) await channel.CloseAsync(cancellationToken);
        _logger.LogInformation("Successfully closed the channel for sending messages to '{ExchangeName}'.", EXCHANGE_NAME);
    }
}