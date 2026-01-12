using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UserManagement.API;
using UserManagement.Infrastructure.Messaging.Options;
using UserManagement.Persistence;
using Testcontainers.RabbitMq;
using Testcontainers.Papercut;
using Testcontainers.PostgreSql;
using FluentEmail.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentEmail.Smtp;
using System.Net.Mail;

namespace UserManagement.Application.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .Build();
    
    private const string RabbitMQUsername = "test";
    private const string RabbitMQPassword = "test";
    private readonly RabbitMqContainer _rabbitMQContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.2.0-management")
        .WithUsername(RabbitMQUsername)
        .WithPassword(RabbitMQPassword)
        .Build();

    private readonly PapercutContainer _papercutContainer = new PapercutBuilder()
        .WithImage("changemakerstudiosus/papercut-smtp:latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMQContainer.StartAsync();
        await _papercutContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            ConfigureMSSqlServer(services);

            ConfigureRabbitMQ(services);

            ConfigurePapercut(services);
        });
    }

    public async new Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _rabbitMQContainer.StopAsync();
        await _papercutContainer.StopAsync();
    }

    private void ConfigurePapercut(IServiceCollection services)
    {
        services.RemoveAll<FluentEmail.Core.Interfaces.ISender>();

        services.AddFluentEmail("integration@tests.com", "IntegrationTests")
                .AddSmtpSender(() => new SmtpClient(
                    _papercutContainer.Hostname,
                    _papercutContainer.GetMappedPublicPort(2525)
                ));

        services.TryAdd(ServiceDescriptor.Singleton((Func<IServiceProvider, ISender>)((IServiceProvider _) => 
            new SmtpSender(new SmtpClient(_papercutContainer.Hostname, _papercutContainer.GetMappedPublicPort(2525))))));
    }

    private void ConfigureRabbitMQ(IServiceCollection services)
    {
        var rebbitMqOptionsDesctiptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(IConfigureOptions<RabbitMQOptions>));
        
        if (rebbitMqOptionsDesctiptor is not null)
        {
            services.Remove(rebbitMqOptionsDesctiptor);
        }

        var rabbitMqOptions = new RabbitMQOptions()
        {
            HostName = _rabbitMQContainer.Hostname,
            Port = _rabbitMQContainer.GetMappedPublicPort(5672),
            UserName = RabbitMQUsername,
            Password = RabbitMQPassword
        };
        
        services.AddSingleton<RabbitMQOptions>(rabbitMqOptions);
        services.AddSingleton<IOptions<RabbitMQOptions>>(sp =>
            Microsoft.Extensions.Options.Options.Create(sp.GetRequiredService<RabbitMQOptions>()));
    }

    private void ConfigureMSSqlServer(IServiceCollection services)
    {
        var dbContextDescriptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<ApplicationContext>));
        
        if (dbContextDescriptor is not null)
        {
            services.Remove(dbContextDescriptor);
        }

        services.AddDbContext<ApplicationContext>(options =>
        {
            options
                .UseNpgsql(
                    _dbContainer.GetConnectionString(),
                    contextOptions =>
                        {
                            contextOptions.EnableRetryOnFailure(
                                maxRetryCount: 10,
                                maxRetryDelay: TimeSpan.FromSeconds(5),
                                errorCodesToAdd: null
                            );
                        });
        });

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var appContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        appContext.Database.Migrate();
    }
}