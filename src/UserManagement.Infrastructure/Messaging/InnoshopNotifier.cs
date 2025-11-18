using System.Text;
using System.Text.Json;
using Innoshop.Contracts.UserManagement;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Messaging;

public class InnoshopNotifier : IInnoshopNotifier
{
    private readonly ILogger<InnoshopNotifier> _logger;
    private IChannel _channel;

    public InnoshopNotifier(
        ILogger<InnoshopNotifier> logger,
        RabbitMQConnection connection)
    {
        if (connection.Channel == null) throw new Exception("");
        
        _logger = logger;
        _channel = connection.Channel;
    }

    public async Task SendTokenRevokedNotificationAsync(TokenRevokedMessage message)
    {
        await SendNotification(TokenRevokedMessage.QueueName, message);
        _logger.LogInformation("Sent notification of revoking access token with ID '{TokenId}'", message.TokenId);
    }

    public async Task SendUserDeactivatedNotificationAsync(UserDeactivatedMessage message)
    {
        await SendNotification(UserDeactivatedMessage.QueueName, message);
        _logger.LogInformation("Sent notification of deactivating user with ID '{UserId}'", message.UserId);
    }

    public async Task SendUserReactivatedNotificationAsync(UserReactivatedMessage message)
    {
        await SendNotification(UserReactivatedMessage.QueueName, message);
        _logger.LogInformation("Sent notification of reactivating user with ID '{UserId}'", message.UserId);
    }

    public async Task SendUserDeletedNotificationAsync(UserDeletedMessage message)
    {
        await SendNotification(UserDeletedMessage.QueueName, message);
        _logger.LogInformation("Sent notification of deleting user with ID '{UserId}'", message.UserId);
    }

    private async Task SendNotification(string queue, object notification)
    {
        var json = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(json);
        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queue,
            body: body
        );
    }
}