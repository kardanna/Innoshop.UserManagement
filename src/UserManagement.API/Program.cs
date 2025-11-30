using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserManagement.API.OptionsSetup;
using UserManagement.Persistence;
using UserManagement.Infrastructure;
using UserManagement.Application;
using UserManagement.Presentation;

namespace UserManagement.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();


        //Policies, Services, PipelineBehaviour, PasswordHasher        
        builder.Services.AddUserManagemenetApplication();

        //MediatR
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(UserManagement.Application.AssemblyReference.Assembly));

        //Validation behaviour
        builder.Services.AddValidatorsFromAssembly(UserManagement.Application.AssemblyReference.Assembly,
            includeInternalTypes: true);


        //Authentication, Messaging, EmailSender, BackgroundJobs
        builder.Services.AddUserManagementInfrastructure();
        builder.Services
            .AddFluentEmail(
                builder.Configuration["UserManagement:EmailOptions:EmailSender"],
                builder.Configuration["UserManagement:EmailOptions:Sender"])
            .AddSmtpSender(
                builder.Configuration["Papercut:HostName"],
                builder.Configuration.GetValue<int>("Papercut:Port"));


        //DbContext, UnitOfWork, Repositories
        builder.Services.AddUserManagementPersistence();
        builder.Services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("SqlServer"),
                contextOptions =>
                {
                    contextOptions.EnableRetryOnFailure(
                        maxRetryCount: 6,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
        });

        //AddControllers, UrlProvider, GlobalExceptionHandler, AddHttpContextAccessor
        builder.Services.AddUserManagementPresentation();

        
        //Options
        builder.Services.ConfigureOptions<RabbitMQOptionsSetup>();
        builder.Services.ConfigureOptions<LoginOptionsSetup>();
        builder.Services.ConfigureOptions<RegistrationOptionsSetup>();
        builder.Services.ConfigureOptions<EmailOptionsSetup>();
        builder.Services.ConfigureOptions<PasswordOptionsSetup>();
        builder.Services.ConfigureOptions<JwtOptionsSetup>();
        builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
        builder.Services.ConfigureOptions<SigningKeyOptionsSetup>();
        
        
        //Logging
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            db.Database.Migrate();
        }
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseSerilogRequestLogging();

        app.UseExceptionHandler(options => { });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
