using Innoshop.Contracts.UserManagement.UserEvents;

namespace UserManagement.Application.Interfaces;

public interface IInnoshopNotifier
{
    Task SendUserDeactivatedNotificationAsync(UserDeactivatedMessage message);
    Task SendUserReactivatedNotificationAsync(UserReactivatedMessage message);
    Task SendUserDeletedNotificationAsync(UserDeletedMessage message);
}