using Microsoft.Extensions.Options;
using UserManagement.Application.Options;

namespace UserManagement.API.OptionsSetup;

public class LoginOptionsSetup : IConfigureOptions<LoginOptions>
{
    private const string SECTION_NAME = "LoginOptions"; 
    private readonly IConfiguration _configuration;

    public LoginOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(LoginOptions options)
    {
        _configuration.GetSection(SECTION_NAME).Bind(options);
    }
}