namespace UserManagement.Infrastructure.EmailSender.Options;

public class PapercutOptions
{
    public string HostName { get; init; } = null!;
    public int Port { get; init; }
}