using Microsoft.Extensions.Hosting;

namespace UserManagement.Infrastructure.Messaging;

public class RabbitMQConnectionInitializer : IHostedService
{
    private readonly RabbitMQConnection _connection;

    public RabbitMQConnectionInitializer(RabbitMQConnection connection)
    {
        _connection = connection;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _connection.Initialize();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _connection.DisposeAsync();
    }
}