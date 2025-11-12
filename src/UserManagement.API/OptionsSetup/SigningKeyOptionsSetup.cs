using Microsoft.Extensions.Options;
using UserManagement.Infrastructure.Authentication.Configuration;

namespace UserManagement.API.OptionsSetup;

public class SigningKeyOptionsSetup : IConfigureOptions<SigningKeyOptions>
{
    private const string SectionName = "SigningKeyOptions";
    private readonly IConfiguration _configuration;

    public SigningKeyOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(SigningKeyOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}