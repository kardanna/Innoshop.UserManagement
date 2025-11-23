using Innoshop.Contracts.UserManagement;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Messaging;

public class InnoshopNotifier : IInnoshopNotifier
{
    private readonly ILogger<InnoshopNotifier> _logger;
    private readonly UserManagementExchange _exchange;

    public InnoshopNotifier(
        ILogger<InnoshopNotifier> logger,
        UserManagementExchange exchange)
    {

        _logger = logger;
        _exchange = exchange;
    }

    public async Task SendTokenRevokedNotificationAsync(TokenRevokedMessage message)
    {
        await _exchange.SendMessage(TokenRevokedMessage.RoutingKey, message);
        _logger.LogInformation("Sent notification of revoking access token with ID '{TokenId}'", message.TokenId);
    }

    public async Task SendUserDeactivatedNotificationAsync(UserDeactivatedMessage message)
    {
        await _exchange.SendMessage(UserDeactivatedMessage.RoutingKey, message);
        _logger.LogInformation("Sent notification of deactivating user with ID '{UserId}'", message.UserId);
    }

    public async Task SendUserReactivatedNotificationAsync(UserReactivatedMessage message)
    {
        await _exchange.SendMessage(UserReactivatedMessage.RoutingKey, message);
        _logger.LogInformation("Sent notification of reactivating user with ID '{UserId}'", message.UserId);
    }

    public async Task SendUserDeletedNotificationAsync(UserDeletedMessage message)
    {
        await _exchange.SendMessage(UserDeletedMessage.RoutingKey, message);
        _logger.LogInformation("Sent notification of deleting user with ID '{UserId}'", message.UserId);
    }
}