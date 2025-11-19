using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Application.UseCases.Passwords.Change;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Errors;
using UserManagement.Domain.Shared;
using UserManagement.Presentation.DTOs;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers;

[Route("user")]
public class ChangePasswordController : BaseApiController
{
    private readonly ILogger<ChangePasswordController> _logger;

    public ChangePasswordController(
        ILogger<ChangePasswordController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPost("me/changepassword")]
    [Authorize(Roles = nameof(Role.Customer) + "," + nameof(Role.Administrator))]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }

        var command = new ChangePasswordCommand(
            userId: userGuid,
            oldPassword: request.OldPassword,
            newPassword: request.NewPassword
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}