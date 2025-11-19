using Microsoft.Extensions.Options;
using UserManagement.Application.Options;

namespace UserManagement.API.OptionsSetup;

public class PasswordOptionsSetup : IConfigureOptions<PasswordOptions>
{
    private const string SECTION_NAME = "PasswordOptions";
    private readonly IConfiguration _configuration;

    public PasswordOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(PasswordOptions options)
    {
        _configuration.GetSection(SECTION_NAME).Bind(options);
    }
}