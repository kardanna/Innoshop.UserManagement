using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.EmailSender;

public static class EmailSenderDependencyInjection
{
    public static void AddEmailSender(this IServiceCollection services)
    {
        services.AddTransient<IEmailSender, EmailSender>();
    }
}