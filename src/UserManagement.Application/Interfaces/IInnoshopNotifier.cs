namespace UserManagement.Application.Interfaces;

public interface IInnoshopNotifier
{
    Task SendTokenRevokedNotificationAsync(Guid tokenId, DateTime tokenExpiresAtUtc);
    Task SendUserDeactivatedNotificationAsync(Guid userId);
    Task SendUserDeletedNotificationAsync(Guid userId);
}