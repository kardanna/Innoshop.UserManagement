using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Application.UseCases.Users.Deactivate;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers;

[Route("user")]
public class DeactivateUserController : BaseApiController
{
    private readonly ILogger<DeactivateUserController> _logger;

    public DeactivateUserController(
        ILogger<DeactivateUserController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPost("me/deactivate")]
    [Authorize(Roles = nameof(Role.Customer))]
    public async Task<IActionResult> Deactivate()
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var command = new DeactivateUserCommand(
            userId: userGuid,
            requesterId: userGuid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }

    [HttpPost("{userId:guid}/deactivate")]
    [Authorize(Roles = nameof(Role.Administrator))]
    public async Task<IActionResult> Deactivate(Guid userId)
    {
        var requesterId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(requesterId, out var requesterGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var command = new DeactivateUserCommand(
            userId: userId,
            requesterId: requesterGuid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}