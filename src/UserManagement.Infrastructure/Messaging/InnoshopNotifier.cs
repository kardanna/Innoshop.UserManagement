using Innoshop.Contracts.UserManagement.UserEvents;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

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

    public async Task<Result> SendUserDeactivatedNotificationAsync(UserDeactivatedMessage message)
    {
        try
        {
            _logger.LogInformation("Sending notification of deactivating user with ID '{UserId}'...", message.UserId);
            await _exchange.SendMessage(UserDeactivatedMessage.Topic, message);
            _logger.LogInformation("Successfully sent notification of deactivating user with ID '{UserId}'.", message.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sent notification of deactivating user with ID '{UserId}'", message.UserId);
            return Result.Failure(DomainErrors.Deactivation.FailedToSendNotification);
        }
    }

    public async Task<Result> SendUserReactivatedNotificationAsync(UserReactivatedMessage message)
    {
        try
        {
            _logger.LogInformation("Sending notification of reactivating user with ID '{UserId}'...", message.UserId);
            await _exchange.SendMessage(UserReactivatedMessage.Topic, message);
            _logger.LogInformation("Successfully sent notification of reactivating user with ID '{UserId}'.", message.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sent notification of reactivating user with ID '{UserId}'", message.UserId);
            return Result.Failure(DomainErrors.Reactivation.FailedToSendNotification);
        }
    }

    public async Task<Result> SendUserDeletedNotificationAsync(UserDeletedMessage message)
    {
        try
        {
            _logger.LogInformation("Sending notification of deleting user with ID '{UserId}'...", message.UserId);
            await _exchange.SendMessage(UserDeletedMessage.Topic, message);
            _logger.LogInformation("Successfully sent notification of deleting user with ID '{UserId}'.", message.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sent notification of deleting user with ID '{UserId}'", message.UserId);
            return Result.Failure(DomainErrors.Deletion.FailedToSendNotification);
        }
    }
}