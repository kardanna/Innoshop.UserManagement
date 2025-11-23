using Microsoft.Extensions.Hosting;
using UserManagement.Infrastructure.Messaging.Abstractions;

namespace UserManagement.Infrastructure.Messaging;

public class ExchangeChannelInitializer : IHostedService
{
    private readonly IExchangeChannel _exchangeChannel;

    public ExchangeChannelInitializer(IExchangeChannel exchangeChannel)
    {
        _exchangeChannel = exchangeChannel;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _exchangeChannel.Initialize();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _exchangeChannel.DisposeAsync();
    }
}