using Microsoft.Extensions.Options;
using UserManagement.Application.Options;

namespace UserManagement.API.OptionsSetup;

public class EmailOptionsSetup : IConfigureOptions<EmailOptions>
{
    private const string SECTION_NAME = "EmailOptions";
    private readonly IConfiguration _configuration;

    public EmailOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(EmailOptions options)
    {
        _configuration.GetSection(SECTION_NAME).Bind(options);
    }
}