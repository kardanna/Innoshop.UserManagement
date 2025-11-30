using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Interfaces;
using UserManagement.Infrastructure.Messaging.Abstractions;

namespace UserManagement.Infrastructure.Messaging;

public static class MessagingDependencyInjection
{
    public static void AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<RabbitMQConnectionProvider>();
        services.AddSingleton<IRabbitMQConnectionProvider>(sp => sp.GetRequiredService<RabbitMQConnectionProvider>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQConnectionProvider>());
        
        services.AddSingleton<RabbitMQConfigurator>();
        services.AddSingleton<IRabbitMQConfigurator>(sp => sp.GetRequiredService<RabbitMQConfigurator>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQConfigurator>());
        
        services.AddSingleton<UserEventsExchange>();
        services.AddHostedService(sp => sp.GetRequiredService<UserEventsExchange>());
        services.AddSingleton<IInnoshopNotifier, InnoshopNotifier>();
    }
}