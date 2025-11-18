using Innoshop.Contracts.UserManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace UserManagement.Infrastructure.Messaging;

public class RabbitMQConnection : IAsyncDisposable
{
    private readonly ILogger<RabbitMQConnection> _logger;
    private readonly RabbitMQOptions _options;

    public IConnection? Connection { get; private set; }
    public IChannel? Channel { get; private set; }

    public RabbitMQConnection(
        ILogger<RabbitMQConnection> logger,
        IOptions<RabbitMQOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task Initialize()
    {
        if (Channel != null) return;

        var factory = new ConnectionFactory()
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        Connection = await factory.CreateConnectionAsync();
        _logger.LogInformation($"Established connection to RabbitMQ");

        Channel = await Connection.CreateChannelAsync();
        _logger.LogInformation($"Created a channel");

        await Channel.QueueDeclareAsync(
            queue: TokenRevokedMessage.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            passive: false,
            noWait: false
        );
        _logger.LogInformation("Created queue '{RabbitMQQueueName}'", TokenRevokedMessage.QueueName);

        await Channel.QueueDeclareAsync(
            queue: UserDeactivatedMessage.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            passive: false,
            noWait: false
        );
        _logger.LogInformation("Created queue '{RabbitMQQueueName}'", UserDeactivatedMessage.QueueName);

        await Channel.QueueDeclareAsync(
            queue: UserReactivatedMessage.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            passive: false,
            noWait: false
        );
        _logger.LogInformation("Created queue '{RabbitMQQueueName}'", UserReactivatedMessage.QueueName);

        await Channel.QueueDeclareAsync(
            queue: UserDeletedMessage.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            passive: false,
            noWait: false
        );
        _logger.LogInformation("Created queue '{RabbitMQQueueName}'", UserDeletedMessage.QueueName);
    }

    public async ValueTask DisposeAsync()
    {
        if (Channel != null) await Channel.CloseAsync();
        _logger.LogInformation($"Closed RabbitMQ channel");

        if (Connection != null) await Connection.CloseAsync();
        _logger.LogInformation($"Closed RabbitMQ connection");
    }
}