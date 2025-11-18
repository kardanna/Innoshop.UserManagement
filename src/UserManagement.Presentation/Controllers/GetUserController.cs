using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers;

[Route("user")]
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
    [Authorize(Roles = nameof(Role.Administrator) + "," + nameof(Role.Customer))]
    public async Task<IActionResult> GetMe()
    {
        var id = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (id == null || !Guid.TryParse(id, out var guid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var query = new GetUserQuery(guid);

        var response = await _sender.Send(query);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator")]
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