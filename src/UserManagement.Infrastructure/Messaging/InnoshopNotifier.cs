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
        Console.WriteLine($"Sent notification of revoking access token with ID '{message.TokenId}' that expires at {message.TokenExpiresAtUtc}");
        //throw new NotImplementedException();
    }

    public async Task SendUserDeactivatedNotificationAsync(UserDeactivatedMessage message)
    {
        //throw new NotImplementedException();
    }

    public async Task SendUserDeletedNotificationAsync(UserDeletedMessage message)
    {
        //throw new NotImplementedException();
    }

    public async Task SendUserReactivatedNotificationAsync(UserReactivatedMessage message)
    {
        //throw new NotImplementedException();
    }
}