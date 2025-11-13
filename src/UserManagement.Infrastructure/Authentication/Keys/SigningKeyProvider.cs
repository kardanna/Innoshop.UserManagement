using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Authentication.Configuration;

namespace UserManagement.Infrastructure.Authentication.Keys;

public class SigningKeyProvider : ISigningKeyProvider
{
    public const string PROTECTOR_NAME = "JWT_Private_Key_Protector"; //Move to domain???
    private readonly ISigningKeyCache _cache;
    private readonly ISigningKeyRecordRepository _repository;
    private readonly SigningKeyOptions _options;
    private readonly IDataProtector _protector;
    private readonly IUnitOfWork _unitOfWork;

    public SigningKeyProvider(
        ISigningKeyCache cache,
        ISigningKeyRecordRepository repository,
        IOptions<SigningKeyOptions> options,
        IDataProtectionProvider protector,
        IUnitOfWork unitOfWork)
    {
        _cache = cache;
        _repository = repository;
        _options = options.Value;
        _protector = protector.CreateProtector(PROTECTOR_NAME);
        _unitOfWork = unitOfWork;
    }

    public async Task InitializeSigningKeysCacheAsync()
    {
        if (_cache.IsEmpty)
        {
            var records = await _repository
                .GetValidSigningKeysAsync(_options.ValidationKeyLifetimeDays);
            foreach (var record in records)
            {
                _cache.TryAddKey(SigningKeyFromRecord(record));
            }
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

            var (keyPair, keyPairRecord) = CreateSigningKey();

            _repository.Add(keyPairRecord);

            if (_cache.TryAddKey(keyPair))
            {
                await _unitOfWork.SaveChangesAsync();
                return keyPair.PrivateKey;
            }

            throw new Exception("Unable to generate signing key"); //!!!!!!!!!!!!!!!!!!    
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