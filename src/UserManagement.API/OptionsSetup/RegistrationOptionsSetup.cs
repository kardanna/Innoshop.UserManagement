using Microsoft.Extensions.Options;
using UserManagement.Application.Options;

namespace UserManagement.API.OptionsSetup;

public class RegistrationOptionsSetup : IConfigureOptions<RegistrationOptions>
{
    private const string SECTION_NAME = "RegistrationOptions";
    private readonly IConfiguration _configuration;

    public RegistrationOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(RegistrationOptions options)
    {
        _configuration.GetSection(SECTION_NAME).Bind(options);
    }
}