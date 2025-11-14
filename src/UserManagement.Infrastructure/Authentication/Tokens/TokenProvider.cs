using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Users.Login;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;
using UserManagement.Infrastructure.Authentication.Configuration;
using UserManagement.Infrastructure.Authentication.Keys;
using UserManagement.Infrastructure.Authentication.Repositories;

namespace UserManagement.Infrastructure.Authentication.Tokens;

public class TokenProvider : ITokenProvider
{
    private readonly ITokenRecordRepository _tokenRepository;
    private readonly ISigningKeyProvider _signingKeyProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    public TokenProvider(
        ITokenRecordRepository tokenRepository,
        ISigningKeyProvider signingKeyProvider,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _tokenRepository = tokenRepository;
        _signingKeyProvider = signingKeyProvider;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<LoginUserResponse>> GenerateFromLogin(User user)
    {
        var signingKey = await _signingKeyProvider.GetSigningKeyAsync();

        var tokenRecord = CreateTokenRecord(user);

        List<Claim> claims = user.Roles
            .DistinctBy(c => c.Name)
            .Select(r => new Claim(ClaimTypes.Role, r.Name))
            .ToList();

        claims.Add(new(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.Email, user.Email));
        claims.Add(new(JwtRegisteredClaimNames.Jti, tokenRecord.AccessTokenId.ToString()));

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            null,
            tokenRecord.AccessTokenExpiresAt,
            signingCredentials
        );

        string accessTokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        await _unitOfWork.SaveChangesAsync();

        return new LoginUserResponse(
            accessTokenValue,
            tokenRecord.AccessTokenExpiresAt,
            tokenRecord.RefreshToken
        );
    }

    public async Task<Result<LoginUserResponse>> GenerateFromRefreshToken(string refreshToken)
    {
        var tokenRecord = await _tokenRepository.GetAsync(refreshToken);

        if (tokenRecord == null) return DomainErrors.RefreshToken.NotFound;

        var isRefreshTokenExpired = tokenRecord.IssuedAt.AddMinutes(_jwtOptions.RefreshTokenLifetimeMinutes) < DateTime.UtcNow;
        if (isRefreshTokenExpired)
        {
            _tokenRepository.Revome(tokenRecord.AccessTokenId);
            await _unitOfWork.SaveChangesAsync();

            return DomainErrors.RefreshToken.Expired;
        }

        var newToken = await GenerateFromLogin(tokenRecord.User);
        //if old access! token has not yet expired - post a message saying it's invalid!!!!!!!!!!!!
        //and to own redis
        _tokenRepository.Revome(tokenRecord.AccessTokenId);
        await _unitOfWork.SaveChangesAsync();

        return newToken;
    }

    private TokenRecord CreateTokenRecord(User user)
    {
        var tokenRecord = new TokenRecord(
            accessTokenId: Guid.CreateVersion7(),
            accessTokenLifetime: TimeSpan.FromMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            userId: user.Id,
            refreshToken: GenerateRefreshToken()
        );

        _tokenRepository.Add(tokenRecord);

        return tokenRecord;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[256];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}