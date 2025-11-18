using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserManagement.Presentation.Behaviours;
using UserManagement.Presentation.ExceptionHandlers;
using UserManagement.API.OptionsSetup;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Policies;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Authentication.Keys;
using UserManagement.Infrastructure.Authentication.Repositories;
using UserManagement.Infrastructure.Authentication.Tokens;
using UserManagement.Infrastructure.Messaging;
using UserManagement.Persistence;
using UserManagement.Persistence.Repositories;

namespace UserManagement.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddApplicationPart(UserManagement.Presentation.AssemblyReference.Assembly);
        
        builder.Services.AddOpenApi();

        //Database
        builder.Services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("SqlServer"),
                contextOptions =>
                {
                    contextOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                });
        });
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        //Scoped
        builder.Services.AddScoped<ISigningKeyRecordRepository, SigningKeyRecordsRepository>();
        builder.Services.AddScoped<ITokenRecordRepository, TokenRecordRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        builder.Services.AddScoped<IEmailVerificationAttemptRepository, EmailVerificationAttemptRepository>();
        builder.Services.AddScoped<IUserPolicy, UserPolicy>();
        builder.Services.AddScoped<IEmailPolicy, EmailPolicy>();

        //Singletons
        builder.Services.AddSingleton<ISigningKeyCache, SigningKeysCache>();
        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        //Scoped services
        builder.Services.AddScoped<ISigningKeyProvider, SigningKeyProvider>();
        builder.Services.AddScoped<ITokenProvider, TokenProvider>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        //Configuring options
        builder.Services.ConfigureOptions<JwtOptionsSetup>();
        builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
        builder.Services.ConfigureOptions<SigningKeyOptionsSetup>();
        builder.Services.ConfigureOptions<LoginOptionsSetup>();
        builder.Services.ConfigureOptions<RegistrationOptionsSetup>();
        builder.Services.ConfigureOptions<EmailOptionsSetup>();
        builder.Services.ConfigureOptions<RabbitMQOptionsSetup>();

        //Hosted services
        builder.Services.AddHostedService<SigningKeyCacheInitializer>();

        //RabbitMQ
        builder.Services.AddSingleton<RabbitMQConnection>();
        builder.Services.AddHostedService<RabbitMQConnectionInitializer>();
        builder.Services.AddSingleton<IInnoshopNotifier, InnoshopNotifier>();

        //Authentication configuration
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);

        //Validation behaviour
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PipelineValidationBehaviour<,>));
        builder.Services.AddValidatorsFromAssembly(UserManagement.Application.AssemblyReference.Assembly,
            includeInternalTypes: true);
        
        //Data protection configuration
        builder.Services.AddDataProtection()
            .SetApplicationName("Innoshop.UserManagement");

        //Global exception handler
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        //Logging
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });

        //MediatR
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(UserManagement.Application.AssemblyReference.Assembly));
        

        var app = builder.Build();


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
