using UserManagement.Domain.Entities;

namespace UserManagement.Application.Repositories;

public interface IPasswordRestoreAttemptRepository
{
    void Add(PasswordRestoreAttempt attempt);
    void RemovePreviousUnseccessfulAttempts(Guid userId);
    Task<PasswordRestoreAttempt?> GetAsync(string attemptCode);
}