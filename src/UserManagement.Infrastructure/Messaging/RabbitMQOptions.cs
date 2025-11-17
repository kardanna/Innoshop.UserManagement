namespace UserManagement.Infrastructure.Messaging;

public class RabbitMQOptions
{
    public string HostName { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
}