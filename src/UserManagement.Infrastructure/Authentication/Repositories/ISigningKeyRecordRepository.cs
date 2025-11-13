using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Authentication.Repositories;

public interface ISigningKeyRecordRepository
{
    Task<IEnumerable<SigningKeyRecord>> GetValidSigningKeysAsync(int numberOfDaysKeyIsValid);
    void Add(SigningKeyRecord key);
    void RemoveExpiredKeys(int numberOfDaysKeyIsValid);
}