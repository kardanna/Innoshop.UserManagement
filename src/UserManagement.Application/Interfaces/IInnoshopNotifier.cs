using Innoshop.Contracts.UserManagement;

namespace UserManagement.Application.Interfaces;

public interface IInnoshopNotifier
{
    Task SendTokenRevokedNotificationAsync(TokenRevokedMessage message);
    Task SendUserDeactivatedNotificationAsync(UserDeactivatedMessage message);
    Task SendUserReactivatedNotificationAsync(UserReactivatedMessage message);
    Task SendUserDeletedNotificationAsync(UserDeletedMessage message);
}