using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using UserManagement.Application.Interfaces;
using UserManagement.Presentation.Controllers;

namespace UserManagement.Presentation;

public class UrlProvider : IUrlProvider
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UrlProvider(
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUrlForEmailVerificationEndpoint(string verificationCode)
    {
        if (_httpContextAccessor.HttpContext is null) return null;

        return _linkGenerator.GetUriByAction(
            httpContext: _httpContextAccessor.HttpContext,
            action: nameof(EmailController.VerifyEmail),
            controller: nameof(EmailController).Replace("Controller", ""),
            values: new { code = verificationCode }
        );
    }
}