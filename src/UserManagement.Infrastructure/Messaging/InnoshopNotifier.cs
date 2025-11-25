using Innoshop.Contracts.UserManagement.UserEvents;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Messaging;

public class InnoshopNotifier : IInnoshopNotifier
{
    private readonly ILogger<InnoshopNotifier> _logger;
    private readonly UserEventsExchange _exchange;

    public InnoshopNotifier(
        ILogger<InnoshopNotifier> logger,
        UserEventsExchange exchange)
    {

        _logger = logger;
        _exchange = exchange;
    }

    public async Task SendUserDeactivatedNotificationAsync(UserDeactivatedMessage message)
    {
        await _exchange.SendMessage(UserDeactivatedMessage.Topic, message);
        _logger.LogInformation("Sent notification of deactivating user with ID '{UserId}'", message.UserId);
    }

    public async Task SendUserReactivatedNotificationAsync(UserReactivatedMessage message)
    {
        await _exchange.SendMessage(UserReactivatedMessage.Topic, message);
        _logger.LogInformation("Sent notification of reactivating user with ID '{UserId}'", message.UserId);
    }

    public async Task SendUserDeletedNotificationAsync(UserDeletedMessage message)
    {
        await _exchange.SendMessage(UserDeletedMessage.Topic, message);
        _logger.LogInformation("Sent notification of deleting user with ID '{UserId}'", message.UserId);
    }
}