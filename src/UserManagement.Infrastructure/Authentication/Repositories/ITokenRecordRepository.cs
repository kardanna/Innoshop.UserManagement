using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Authentication.Repositories;

public interface ITokenRecordRepository
{
    void Add(TokenRecord record);
    Task<TokenRecord?> GetAsync(Guid accessTokenId);
    Task<TokenRecord?> GetAsync(string refreshToken);
    Task<IEnumerable<TokenRecord>> GetAllAsync(Guid userId);
    void Revome(Guid accessTokenId);
    void Revome(string refreshToken);
}