using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.API.DTOs;
using UserManagement.Application.Users.Get;
using UserManagement.Application.Users.Logout;
using UserManagement.Application.Users.LogoutEverywhere;
using UserManagement.Application.Users.Registration;
using UserManagement.Application.Users.Update;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;

namespace UserManagement.API.Controllers;

[Route("user")]
public class UserController : BaseApiController
{
    private readonly ILogger<AuthController> _logger;

    public UserController(
        ILogger<AuthController> logger,
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
        
        Guid.TryParse(requesterId, out var requesterGuid);

        var query = new GetUserQuery(id, requesterGuid);

        var response = await _sender.Send(query);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
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

        return CreatedAtAction(nameof(GetById), new { id = response.Value.id } , response.Value);
    }

    [HttpPut("me")]
    [Authorize(Roles = nameof(Role.Customer))]
    public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
    {
        var id = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (id == null || !Guid.TryParse(id, out var guid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var command = new UpdateUserCommand(
            guid,
            request.FirstName,
            request.LastName,
            request.DateOfBirth
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
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