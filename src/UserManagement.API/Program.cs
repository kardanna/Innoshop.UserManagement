using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.API.Behaviours;
using UserManagement.API.OptionsSetup;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Policies;
using UserManagement.Application.Repositories;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Authentication.Keys;
using UserManagement.Infrastructure.Authentication.Tokens;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Persistence.Repositories;

namespace UserManagement.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
        });
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddScoped<ISigningKeyRecordRepository, SigningKeyRecordsRepository>();
        builder.Services.AddScoped<ITokenRecordRepository, TokenRecordRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        builder.Services.AddScoped<IEmailVerificationAttemptRepository, EmailVerificationAttemptRepository>();
        builder.Services.AddScoped<IRegistrationPolicy, RegistrationPolicy>();
        builder.Services.AddScoped<ILoginAttemptPolicy, LoginAttemptPolicy>();

        builder.Services.AddSingleton<ISigningKeyCache, SigningKeysCache>();
        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        builder.Services.AddScoped<ISigningKeyProvider, SigningKeyProvider>();
        builder.Services.AddScoped<ITokenProvider, TokenProvider>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();

        builder.Services.ConfigureOptions<JwtOptionsSetup>();
        builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
        builder.Services.ConfigureOptions<SigningKeyOptionsSetup>();
        builder.Services.ConfigureOptions<LoginOptionsSetup>();
        builder.Services.ConfigureOptions<RegistrationOptionsSetup>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);


        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PipelineValidationBehaviour<,>));
        builder.Services.AddValidatorsFromAssembly(UserManagement.Application.AssemblyReference.Assembly,
            includeInternalTypes: true);
        
        builder.Services.AddDataProtection()
            .SetApplicationName("Inno_Shop.UserManagement");





        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(UserManagement.Application.AssemblyReference.Assembly));

        var app = builder.Build();

        await InitializeSigningKeysCache(app);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
    
    private static async Task InitializeSigningKeysCache(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<ISigningKeyProvider>();
            await service.InitializeSigningKeysCacheAsync();
        }
    }
}
