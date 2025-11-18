using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Presentation.DTOs;
using UserManagement.Application.UseCases.Users.Register;
using Microsoft.Extensions.Logging;

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

        return CreatedAtAction(nameof(GetUserController.GetById), new { id = response.Value.id } , response.Value);
    }
}