using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Application.UseCases.Users.Logout;
using UserManagement.Application.UseCases.Users.LogoutEverywhere;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers;

[Route("user")]
public class LogoutUserController : BaseApiController
{
    private readonly ILogger<LogoutUserController> _logger;

    public LogoutUserController(
        ILogger<LogoutUserController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPost("me/logout")]
    [Authorize(Roles = nameof(Role.Administrator) + "," + nameof(Role.Customer))]
    public async Task<IActionResult> Logout()
    {
        var tokenId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)
            ?.Value;
        
        if (tokenId == null || !Guid.TryParse(tokenId, out var guid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidJwtIdClaim));
        }

        var command = new LogoutUserCommand(
            guid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }

    [HttpPost("me/logouteverywhere")]
    [Authorize(Roles = nameof(Role.Administrator) + "," + nameof(Role.Customer))]
    public async Task<IActionResult> LogoutEverywhere()
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (userId == null || !Guid.TryParse(userId, out var guid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var command = new LogoutUserEverywhereCommand(
            guid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }

    [HttpPost("{userId:guid}/logouteverywhere")]
    [Authorize(Roles = nameof(Role.Administrator) + "," + nameof(Role.Customer))]
    public async Task<IActionResult> Logout(Guid userId)
    {
        var requesterId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        Guid.TryParse(requesterId, out var requesterGuid);

        var query = new GetUserQuery(userId, requesterGuid);

        var command = new LogoutUserEverywhereCommand(
            userId,
            requesterGuid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}