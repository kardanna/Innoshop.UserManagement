using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.API.DTOs;
using UserManagement.Application.Users.VerifyEmail;
using UserManagement.Application.Users.Registration;
using UserManagement.Application.Users.ChangeEmail;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Domain.Shared;
using UserManagement.Domain.Errors;
using UserManagement.Application.Users.Get;
using UserManagement.Domain.Entities;

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
    public async Task<IActionResult> Get()
    {
        foreach(var claim in HttpContext.User.Claims)
        {
            Console.WriteLine($"type : {claim.Type}, value: {claim.Value}");
        }

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
    public async Task<IActionResult> Get(Guid id)
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
 
    [HttpPost("email/confirm")]
    public async Task<IActionResult> Post([FromBody] VerifyEmailRequest request)
    {
        var command = new VerifyEmailCommand(request.VerificationCode);

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Post([FromBody] RegisterUserRequest request)
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

        return Ok(response.Value);
    }
    
    [Authorize]
    [HttpPost("email/change")]
    public async Task<IActionResult> Post([FromBody] ChangeEmailRequest request)
    {
        foreach (var claim in HttpContext.User.Claims)
        {
            Console.WriteLine($"Type: {claim.Type}, value: {claim.Value}");
        }

        var idString = HttpContext.User.Claims
            .Where(c => c.Type == JwtRegisteredClaimNames.Sub)
            .FirstOrDefault()
            ?.Value;

        if (idString == null || !Guid.TryParse(idString, out var id))
        {
            return HandleFailure(Result.Failure(DomainErrors.User.NotFound));
        }
        
        var command = new ChangeEmailCommand(id, request.NewEmail);

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}
