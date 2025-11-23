using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using UserManagement.Infrastructure.Messaging.Abstractions;
using UserManagement.Infrastructure.Messaging.Options;

namespace UserManagement.Infrastructure.Messaging;

public class RabbitMQChannel : IExchangeChannel
{
    private readonly ILogger<RabbitMQChannel> _logger;
    private readonly RabbitMQOptions _options;
    private  IConnection _connection = null!;
    private  IChannel _channel = null!;

    public IChannel Channel => _channel;
    public bool IsInitialized => _channel is not null;

    public RabbitMQChannel(
        ILogger<RabbitMQChannel> logger,
        IOptions<RabbitMQOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task Initialize()
    {
        if (_channel is not null) return;

        var factory = new ConnectionFactory()
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _logger.LogInformation($"Established connection to RabbitMQ");

        _channel = await _connection.CreateChannelAsync();
        _logger.LogInformation($"Created a channel");

        var exchangeName = Innoshop.Contracts.UserManagement.Exchange.Name;
        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic
        );
        _logger.LogInformation("Declared '{ExchangeName}' exchange", exchangeName);
    }

    public async ValueTask DisposeAsync()
    {
        if (Channel != null) await Channel.CloseAsync();
        _logger.LogInformation($"Closed RabbitMQ channel");

        if (_connection != null) await _connection.CloseAsync();
        _logger.LogInformation($"Closed RabbitMQ connection");
    }
}