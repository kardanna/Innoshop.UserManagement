using Microsoft.Extensions.Options;
using UserManagement.Infrastructure.EmailSender.Options;

namespace UserManagement.API.OptionsSetup;

public class PapercutOptionsSetup : IConfigureOptions<PapercutOptions>
{
    private const string SECTION_NAME = "Papercut";
    private readonly IConfiguration _configuration;

    public PapercutOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(PapercutOptions options)
    {
        _configuration.GetSection(SECTION_NAME).Bind(options);
    }
}