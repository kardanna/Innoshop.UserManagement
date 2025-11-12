using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Authentication.Keys;

public interface ISigningKeyRecordRepository
{
    Task<IEnumerable<SigningKeyRecord>> GetValidSigningKeysAsync(int numberOfDaysKeyIsValid);
    void Add(SigningKeyRecord key);
    void RemoveExpiredKeys(int numberOfDaysKeyIsValid);
}