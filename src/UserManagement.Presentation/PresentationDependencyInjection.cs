using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Interfaces;
using UserManagement.Presentation.ExceptionHandlers;

namespace UserManagement.Presentation;

public static class PresentationDependencyInjection
{
    public static void AddUserManagementPresentation(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(AssemblyReference.Assembly);

        //UrlProvider
        services.AddScoped<IUrlProvider, UrlProvider>();
        services.AddHttpContextAccessor();

        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}