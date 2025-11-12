using UserManagement.Domain.Entities;
using UserManagement.Application.Users.Login;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Interfaces;

public interface ITokenProvider
{
    Task<Result<LoginUserResponse>> GenerateFromLogin(User user);
    Task<Result<LoginUserResponse>> GenerateFromRefreshToken(string refreshToken);
}