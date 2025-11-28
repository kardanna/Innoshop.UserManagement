using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;
using Microsoft.Extensions.Logging;
using Innoshop.Contracts.UserManagement.UserRoles;
using UserManagement.Presentation.Attributes;

namespace UserManagement.Presentation.Controllers;

[Route("users")]
public class GetUserController : BaseApiController
{
    private readonly ILogger<GetUserController> _logger;

    public GetUserController(
        ILogger<GetUserController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpGet("me")]
    [HasRole(nameof(Role.Customer), nameof(Role.Administrator))]
    public async Task<IActionResult> GetMe()
    {
        var id = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(id, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var query = new GetUserQuery(userGuid);

        var response = await _sender.Send(query);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
    }

    [HttpGet("{id:guid}")]
    [HasRole(nameof(Role.Administrator))]
    public async Task<IActionResult> GetById(Guid id)
    {
        var requesterId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(requesterId, out var requesterGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var query = new GetUserQuery(id, requesterGuid);

        var response = await _sender.Send(query);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
    }
}