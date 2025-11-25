namespace UserManagement.Infrastructure.Messaging.Abstractions;

public interface IRabbitMQConfigurator
{
    Task Configure(CancellationToken cancellationToken);
}