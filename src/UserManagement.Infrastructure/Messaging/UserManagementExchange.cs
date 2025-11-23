using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using UserManagement.Domain.Exceptions;
using UserManagement.Infrastructure.Messaging.Abstractions;

namespace UserManagement.Infrastructure.Messaging;

public class UserManagementExchange : IExchange
{
    public const string EXCHANGE_NAME = Innoshop.Contracts.UserManagement.Exchange.Name;

    private readonly ILogger<UserManagementExchange> _logger;
    private readonly IExchangeChannel _channel;
    private readonly BasicProperties _properties = new() { Persistent = true };

    public UserManagementExchange(
        ILogger<UserManagementExchange> logger,
        IExchangeChannel channel)
    {
        if (!channel.IsInitialized) throw new UninitializedExchangeChannelException();

        _logger = logger;
        _channel = channel;
    }

    public async Task SendMessage(string topic, object notification)
    {
        var json = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(json);
        await _channel.Channel.BasicPublishAsync(
            exchange: EXCHANGE_NAME,
            routingKey: topic,
            mandatory: true,
            body: body,
            basicProperties: _properties
        );
    }
}