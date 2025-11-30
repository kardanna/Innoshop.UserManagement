using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Behaviours;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Policies;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;

namespace UserManagement.Application;

public static class ApplicationDependencyInjection
{
    public static void AddUserManagemenetApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserPolicy, UserPolicy>();
        services.AddScoped<IEmailPolicy, EmailPolicy>();
        services.AddScoped<IPasswordPolicy, PasswordPolicy>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PipelineValidationBehaviour<,>));

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
    }
}