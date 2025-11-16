using UserManagement.Application.Interfaces;

namespace UserManagement.Application.Services;

public class InnoshopNotifier : IInnoshopNotifier
{
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