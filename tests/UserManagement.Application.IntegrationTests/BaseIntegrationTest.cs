using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Infrastructure.Messaging.Abstractions;
using UserManagement.Persistence;

namespace UserManagement.Application.IntegrationTests;

public abstract class BaseIntegrationTest
{
    private readonly IServiceScope _scope;
    protected readonly ISender _sender;
    protected readonly ApplicationContext _appContext;
    protected readonly IRabbitMQConnectionProvider _rabbitMQConnectionProvider;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        _appContext = _scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        _rabbitMQConnectionProvider = _scope.ServiceProvider.GetRequiredService<IRabbitMQConnectionProvider>();
    }
}