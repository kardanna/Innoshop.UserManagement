using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;
using Microsoft.Extensions.Logging;
using UserManagement.Presentation.DTOs;
using UserManagement.Application.UseCases.Users.Delete;

namespace UserManagement.Presentation.Controllers;

[Route("user")]
public class DeleteUserController : BaseApiController
{
    private readonly ILogger<DeleteUserController> _logger;

    public DeleteUserController(
        ILogger<DeleteUserController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpDelete("me")]
    [Authorize(Roles = nameof(Role.Administrator) + "," + nameof(Role.Customer))]
    public async Task<IActionResult> DeleteMe([FromBody] DeleteUserRequest request)
    {
        var id = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(id, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var query = new DeleteUserCommand(
            subjectId: userGuid,
            password: request.Password,
            requesterId: userGuid
        );

        var response = await _sender.Send(query);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteById(Guid id)
    {
        var requesterId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(requesterId, out var requesterGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var query = new DeleteUserCommand(
            subjectId: id,
            password: null,
            requesterId: requesterGuid
        );

        var response = await _sender.Send(query);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}