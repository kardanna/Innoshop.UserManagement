using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.API.DTOs;
using UserManagement.Application.Users.EmailConfirmation;
using UserManagement.Application.Users.Registration;

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
}
