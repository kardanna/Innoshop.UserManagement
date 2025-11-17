using Microsoft.Extensions.Options;
using UserManagement.Infrastructure.Messaging;

namespace UserManagement.API;

public class RabbitMQOptionsSetup : IConfigureOptions<RabbitMQOptions>
{
    private const string SECTION_NAME = "RabbitMQ";
    private readonly IConfiguration _configuration;

    public RabbitMQOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(RabbitMQOptions options)
    {
        _configuration.GetSection(SECTION_NAME).Bind(options);
    }
}