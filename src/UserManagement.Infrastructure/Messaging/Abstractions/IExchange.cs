namespace UserManagement.Infrastructure.Messaging.Abstractions;

public interface IExchange
{
    Task SendMessage(string topic, object notification);
}