using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Presentation.DTOs;
using UserManagement.Application.UseCases.Tokens.Refresh;
using UserManagement.Application.UseCases.Users.Login;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers;

[Route("auth")]
public class AuthenticationController : BaseApiController
{
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Post([FromBody] LoginUserRequest request)
    {
        var command = new LoginUserCommand(
            request.Email,
            request.Password,
            request.DeviceFingerprint);

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Post([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);
        
        return Ok(response.Value);
    }
}