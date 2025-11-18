using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Presentation.DTOs;
using UserManagement.Application.UseCases.Users.Register;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Domain.Entities;
using UserManagement.Application.UseCases.Admins.Register;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.Presentation.Controllers;

[Route("user")]
public class RegisterUserController : BaseApiController
{
    private readonly ILogger<RegisterUserController> _logger;

    public RegisterUserController(
        ILogger<RegisterUserController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Email,
            request.Password
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return CreatedAtAction(
            actionName: nameof(GetUserController.GetById),
            controllerName: nameof(GetUserController).Replace("Controller", ""),
            routeValues: new { id = response.Value.id } ,
            value: response.Value
        );
    }

    [HttpPost("admin/register")]
    [Authorize(Roles = nameof(Role.Administrator))]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserRequest request)
    {
        var requesterId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(requesterId, out var requesterGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var command = new RegisterAdminCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Email,
            request.Password,
            requesterGuid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return CreatedAtAction(
            actionName: nameof(GetUserController.GetById),
            controllerName: nameof(GetUserController).Replace("Controller", ""),
            routeValues: new { id = response.Value.id } ,
            value: response.Value
        );
    }
}