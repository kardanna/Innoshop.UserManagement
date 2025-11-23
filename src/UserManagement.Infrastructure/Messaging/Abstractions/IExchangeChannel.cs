using RabbitMQ.Client;

namespace UserManagement.Infrastructure.Messaging.Abstractions;

public interface IExchangeChannel : IAsyncDisposable
{
    public IChannel Channel { get; }
    public bool IsInitialized { get; }
    Task Initialize();
}