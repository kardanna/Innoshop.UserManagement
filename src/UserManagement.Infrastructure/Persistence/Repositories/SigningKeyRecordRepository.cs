using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Authentication.Keys;

namespace UserManagement.Infrastructure.Persistence.Repositories;

public class SigningKeyRecordsRepository : ISigningKeyRecordRepository
{
    private readonly ApplicationContext _appContext;

    public SigningKeyRecordsRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task<IEnumerable<SigningKeyRecord>> GetValidSigningKeysAsync(int numberOfDaysKeyIsValid)
    {
        return await _appContext.SigningKeys
            .Where(sk => sk.IssuedAt > DateTime.UtcNow.AddDays(-numberOfDaysKeyIsValid))
            .ToListAsync();
    }

    public void Add(SigningKeyRecord record)
    {
        _appContext.SigningKeys.Add(record);
    }

    public void RemoveExpiredKeys(int numberOfDaysKeyIsValid)
    {
        var expiredKeys = _appContext.SigningKeys
            .Where(sk => sk.IssuedAt < DateTime.UtcNow.AddDays(-numberOfDaysKeyIsValid));
    
        _appContext.SigningKeys.RemoveRange(expiredKeys);
    }
}