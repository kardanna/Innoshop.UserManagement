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
                if(!_cache.TryAddKey(SigningKeyFromRecord(record)))
                {
                    _logger.LogCritical("Unable to add signing key record '{SigningKeyRecordId}' to the in-memory cache", record.Id);
                    Environment.FailFast($"Unable to add signing key record '{record.Id}' to the in-memory cache");
                }
            }

            _logger.LogInformation("All valid keys successfuly added to the in-memory cache");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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