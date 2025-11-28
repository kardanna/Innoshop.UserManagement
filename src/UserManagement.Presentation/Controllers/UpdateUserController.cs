using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Presentation.DTOs;
using UserManagement.Application.UseCases.Users.Update;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;
using Microsoft.Extensions.Logging;
using Innoshop.Contracts.UserManagement.UserRoles;
using UserManagement.Presentation.Attributes;

namespace UserManagement.Presentation.Controllers;

[Route("users")]
public class UpdateUserController : BaseApiController
{
    private readonly ILogger<UpdateUserController> _logger;

    public UpdateUserController(
        ILogger<UpdateUserController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPut("me")]
    [HasRole(nameof(Role.Customer), nameof(Role.Administrator))]
    public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
    {
        var id = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(id, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var command = new UpdateUserCommand(
            userGuid,
            request.FirstName,
            request.LastName,
            request.DateOfBirth
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
    }
}