using Innoshop.Contracts.UserManagement.UserEvents;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Interfaces;

public interface IInnoshopNotifier
{
    Task<Result> SendUserDeactivatedNotificationAsync(UserDeactivatedMessage message);
    Task<Result> SendUserReactivatedNotificationAsync(UserReactivatedMessage message);
    Task<Result> SendUserDeletedNotificationAsync(UserDeletedMessage message);
}