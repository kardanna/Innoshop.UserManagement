using UserManagement.Domain.Entities;

namespace UserManagement.Application.Repositories;

public interface IUserDeactivationRepository
{
    void Add(UserDeactivation record);
    Task<UserDeactivation?> GetLatestAsync(Guid userId);
    void RemoveAllForUser(Guid userId);
}