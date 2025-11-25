using RabbitMQ.Client;

namespace UserManagement.Infrastructure.Messaging.Abstractions;

public interface IRabbitMQConnectionProvider
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken);
}