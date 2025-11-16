using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Authentication.Configuration;
using UserManagement.Infrastructure.Authentication.Exceptions;
using UserManagement.Infrastructure.Authentication.Repositories;

namespace UserManagement.Infrastructure.Authentication.Keys;

public class SigningKeyProvider : ISigningKeyProvider
{
    public const string PROTECTOR_NAME = "JWT_Private_Key_Protector"; //Move to domain???
    private readonly ISigningKeyCache _cache;
    private readonly ISigningKeyRecordRepository _repository;
    private readonly SigningKeyOptions _options;
    private readonly IDataProtector _protector;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SigningKeyProvider> _logger;

    public SigningKeyProvider(
        ISigningKeyCache cache,
        ISigningKeyRecordRepository repository,
        IOptions<SigningKeyOptions> options,
        IDataProtectionProvider protector,
        IUnitOfWork unitOfWork,
        ILogger<SigningKeyProvider> logger)
    {
        _cache = cache;
        _repository = repository;
        _options = options.Value;
        _protector = protector.CreateProtector(PROTECTOR_NAME);
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task InitializeSigningKeysCacheAsync()
    {
        if (_cache.IsEmpty)
        {
            _logger.LogInformation("Initializing signing key in-memory cache");

            var records = await _repository
                .GetValidSigningKeysAsync(_options.ValidationKeyLifetimeDays);

            foreach (var record in records)
            {
                if(!_cache.TryAddKey(SigningKeyFromRecord(record)))
                {
                    _logger.LogCritical("Unable to add signing key record '{SigningKeyRecordId}' to the in-memory cache", record.Id);
                    Environment.FailFast($"Unable to add signing key record '{record.Id}' to the in-memory cache");
                }
            }

            _logger.LogInformation("All valid keys successfuly added to the in-memory cache");
        }
    }

    public IEnumerable<JsonWebKey> GetJsonWebKeys()
    {
        return _cache
            .GetUnexpiredValidationKeys()
            .Select(k => JsonWebKeyConverter.ConvertFromRSASecurityKey(k));
    }

    public async Task<RsaSecurityKey> GetSigningKeyAsync()
    {
        RsaSecurityKey? key = _cache.GetKeyForSigning();
        if (key != null) return key;

        await _cache.KeyGenerationSemaphore.WaitAsync();
        try
        {
            key = _cache.GetKeyForSigning();
            if (key != null) return key;

            _logger.LogInformation("Valid signing key not found. Generating a new one...");

            var (keyPair, keyPairRecord) = CreateSigningKey();

            _logger.LogInformation("Generated a new key pair with ID '{KeyId}'. Persisting...", keyPair.Id);

            _repository.Add(keyPairRecord);

            if (_cache.TryAddKey(keyPair))
            {
                try
                {
                    await _unitOfWork.SaveChangesAsync();
                    return keyPair.PrivateKey;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Failed to persist new key to a database. Aborting...");
                    Environment.FailFast("Failed to persist new key to a database. Aborting...", ex);
                }
            }

            //Inform somebody if fails to often????

            throw new KeyGenerationException("Unable to add new signing key to a cache");
        }
        finally
        {
            _cache.KeyGenerationSemaphore.Release();
        }
    }

    private (SigningKey, SigningKeyRecord) CreateSigningKey()
    {
        var rsa = RSA.Create(_options.KeySizeBytes);
        var publicKeyPem = rsa.ExportRSAPublicKeyPem();
        var privateKeyPem = rsa.ExportRSAPrivateKeyPem();        
        var encryptedPrivateKeyPem = _protector.Protect(privateKeyPem);

        var publicParameters = rsa.ExportParameters(false);
        var rsaPublicOnly = RSA.Create();
        rsaPublicOnly.ImportParameters(publicParameters);

        var keyId = Guid.CreateVersion7();

        var key = new SigningKey()
        {
            Id = keyId,
            PrivateKey = new RsaSecurityKey(rsa) { KeyId = keyId.ToString() },
            PublicKey = new RsaSecurityKey(rsaPublicOnly),
            IssuedAt = DateTime.UtcNow,
            SigningExpiresAt = DateTime.UtcNow.AddDays(_options.SingingKeyLifetimeDays),
            ExpiresAt = DateTime.UtcNow.AddDays(_options.ValidationKeyLifetimeDays)
        };

        var keyRecord = new SigningKeyRecord()
        {
            Id = key.Id,
            PrivateKeyPem = encryptedPrivateKeyPem,
            PublicKeyPem = publicKeyPem,
            IssuedAt = key.IssuedAt,
        };

        return (key, keyRecord);
    }

    private SigningKey SigningKeyFromRecord(SigningKeyRecord record)
    {
        var privateKeyRsa = RSA.Create();
        privateKeyRsa.ImportFromPem(_protector.Unprotect(record.PrivateKeyPem));

        var publicKeyRsa = RSA.Create();
        publicKeyRsa.ImportFromPem(record.PublicKeyPem);

        return new SigningKey()
        {
            Id = record.Id,
            PrivateKey = new RsaSecurityKey(privateKeyRsa),
            PublicKey = new RsaSecurityKey(publicKeyRsa) { KeyId = record.Id.ToString() },
            IssuedAt = record.IssuedAt,
            SigningExpiresAt = record.IssuedAt.AddDays(_options.SingingKeyLifetimeDays),
            ExpiresAt = record.IssuedAt.AddDays(_options.ValidationKeyLifetimeDays)
        };
    }
}