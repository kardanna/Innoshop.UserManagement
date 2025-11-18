using UserManagement.Domain.Entities;
using UserManagement.Application.UseCases.Users.Login;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Interfaces;

public interface ITokenProvider
{
    Task<Result<LoginUserResponse>> GenerateFromLoginAsync(User user, string deviceFingerprint);
    Task<Result<LoginUserResponse>> GenerateFromRefreshTokenAsync(string refreshToken);
    Task<Result> RevokeTokenAsync(Guid tokenId);
    Task<Result> RevokeAllTokensAsync(Guid userId);
}