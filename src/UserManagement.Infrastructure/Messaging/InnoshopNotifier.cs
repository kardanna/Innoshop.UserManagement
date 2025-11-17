using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    public async Task SendTokenRevokedNotificationAsync(Guid tokenId, DateTime tokenExpiresAtUtc)
    {
        Console.WriteLine($"Sent notification of revoking access token with ID '{tokenId}' that expires at {tokenExpiresAtUtc}");
        //throw new NotImplementedException();
    }

    public async Task SendUserDeactivatedNotificationAsync(Guid userId)
    {
        //throw new NotImplementedException();
    }

    public async Task SendUserDeletedNotificationAsync(Guid userId)
    {
        //throw new NotImplementedException();
    }
}