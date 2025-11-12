using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Repositories;

public class TokenRecordRepository : ITokenRecordRepository
{
    private readonly ApplicationContext _appContext;
    public TokenRecordRepository(ApplicationContext usersContext)
    {
        _appContext = usersContext;
    }

    public void Add(TokenRecord record)
    {
        _appContext.TokenRecords.Add(record);
    }
    public async Task<TokenRecord?> GetAsync(Guid accessTokenId)
    {
        return await _appContext.TokenRecords
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.AccessTokenId == accessTokenId);
    }

    public async Task<TokenRecord?> GetAsync(string refreshToken)
    {
        return await _appContext.TokenRecords
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken);
    }

    public void Revome(Guid accessTokenId)
    {
        var records = _appContext.TokenRecords.Where(r => r.AccessTokenId == accessTokenId);
        _appContext.TokenRecords.RemoveRange(records);
    }

    public void Revome(string refreshToken)
    {
        var records = _appContext.TokenRecords.Where(r => r.RefreshToken == refreshToken);
        _appContext.TokenRecords.RemoveRange(records);
    }
}