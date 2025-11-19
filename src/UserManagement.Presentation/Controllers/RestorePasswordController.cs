using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserManagement.Application.UseCases.Passwords.Restore;
using UserManagement.Domain.Errors;
using UserManagement.Presentation.DTOs;

namespace UserManagement.Presentation.Controllers;

[Route("user")]
public class RestorePasswordController : BaseApiController
{
    private readonly ILogger<ChangePasswordController> _logger;

    public RestorePasswordController(
        ILogger<ChangePasswordController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPost("password/sendrestorecode")]
    public async Task<IActionResult> SendPasswordRestoreCode([FromBody] SendPasswordRestoreCodeRequest request)
    {
        var command = new SendPasswordRestoreCodeCommand(
            email: request.Email
        );

        var response = await _sender.Send(command);

        if (response.IsFailure && response.Error != DomainErrors.User.NotFound)
        {
            return Problem(
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                title: "Server Error",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }

        return Ok();
    }

    [HttpPost("password/restore")]
    public async Task<IActionResult> RestorePassword([FromBody] RestorePasswordRequest request)
    {
        var command = new RestorePasswordCommand(
            restoreCode: request.RestoreCode,
            newPassword: request.NewPassword
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}