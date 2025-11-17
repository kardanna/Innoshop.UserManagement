using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Authentication.Configuration;
using UserManagement.Infrastructure.Authentication.Repositories;

namespace UserManagement.Infrastructure.Authentication.Keys;

public class SigningKeyCacheInitializer : IHostedService
{
    private readonly ISigningKeyCache _cache;
    private readonly ILogger<SigningKeyCacheInitializer> _logger;
    private readonly IServiceScopeFactory _factory;
    private readonly SigningKeyOptions _options;
    private readonly IDataProtector _protector;

    public SigningKeyCacheInitializer(
        ISigningKeyCache cache,
        ILogger<SigningKeyCacheInitializer> logger,
        IServiceScopeFactory factory,
        IOptions<SigningKeyOptions> options,
        IDataProtectionProvider protector)
    {
        _cache = cache;
        _logger = logger;
        _factory = factory;
        _options = options.Value;
        _protector = protector.CreateProtector(SigningKeyProvider.PROTECTOR_NAME);;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_cache.IsEmpty)
        {
            using var scope = _factory.CreateScope();
            var _repository = scope.ServiceProvider.GetRequiredService<ISigningKeyRecordRepository>();

            _logger.LogInformation("Initializing signing key in-memory cache");

            var records = await _repository
                .GetValidSigningKeysAsync(_options.ValidationKeyLifetimeDays);

            foreach (var record in records)
            {
                var key = SigningKeyFromRecord(record);
                if (key is null) continue;

                if(!_cache.TryAddKey(key))
                {
                    _logger.LogCritical("Failed to add signing key record '{SigningKeyRecordId}' to the in-memory cache", record.Id);
                    Environment.FailFast($"Failed to add signing key record '{record.Id}' to the in-memory cache");// THROW!!!!!!!
                }

                _logger.LogInformation("Successfully added RSA key pair record '{SigningKeyRecordId}' to the in-memory cache", record.Id);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private SigningKey? SigningKeyFromRecord(SigningKeyRecord record)
    {
        var publicKey = ConvertPemToSecurityKey(
            pem: record.PublicKeyPem,
            keyId: record.Id
        );

        var privateKey = ConvertPemToSecurityKey(
            pem: record.PrivateKeyPem,
            isPemProtected: true
        );

        if (publicKey is null && privateKey is null)
        {
            _logger.LogError("Failed to import validation and signing keys from RSA pair '{KeyId}'.", record.Id);
            return null;
        }

        if (publicKey is null && privateKey is not null)
        {
            _logger.LogWarning("Failed to import validation key from RSA pair '{KeyId}'. Attempting to recover from signing key...", record.Id);

            try
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(privateKey.Rsa.ExportRSAPublicKeyPem());
                publicKey = new RsaSecurityKey(rsa) { KeyId = record.Id.ToString() };
                _logger.LogInformation("Validation key from RSA pair '{KeyId}' recovered from signing key.", record.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recover validation key from RSA pair '{KeyId}'. Pair will be rejected", record.Id);
                return null;
            }
        }

        if (privateKey is null)
        {
            _logger.LogWarning("Failed to import signing key from RSA pair '{KeyId}'. Key will only be used for validation", record.Id);
        }        

        return new SigningKey()
        {
            Id = record.Id,
            PrivateKey = privateKey ?? new RsaSecurityKey(RSA.Create()),
            PublicKey = publicKey!,
            IssuedAt = record.IssuedAt,
            SigningExpiresAt = privateKey is not null 
                ? record.IssuedAt.AddDays(_options.SingingKeyLifetimeDays)
                : record.IssuedAt,
            ExpiresAt = record.IssuedAt.AddDays(_options.ValidationKeyLifetimeDays)
        };
    }

    private RsaSecurityKey? ConvertPemToSecurityKey(string pem, Guid? keyId = null, bool isPemProtected = false)
    {
        var rsa = RSA.Create();
        RsaSecurityKey? securityKey;

        try
        {
            if (isPemProtected)
            {
                pem = _protector.Unprotect(pem);
            }
            
            rsa.ImportFromPem(pem);            
            securityKey = new RsaSecurityKey(rsa);

            if (keyId != null)
            {
                securityKey.KeyId = keyId.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to convert PEM of key {KeyId} to RSA", keyId);
            securityKey = null;
        }

        return securityKey;
    }
}