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

namespace UserManagement.API.Controllers;

[Route("[controller]")]
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
