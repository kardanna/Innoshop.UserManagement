using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Presentation.DTOs;
using UserManagement.Application.UseCases.EmailAddresses.Change;
using UserManagement.Application.UseCases.EmailAddresses.Verify;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Domain.Shared;
using UserManagement.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers;

[Route("users/email")]
public class EmailController : BaseApiController
{
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        ILogger<EmailController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

 
    [HttpGet("verify/{code:required}")]
    public async Task<IActionResult> VerifyEmail(string code)
    {
        var command = new VerifyEmailAddressCommand(code);

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
    
    [Authorize]
    [HttpPost("change")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
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
        
        var command = new ChangeEmailAddressCommand(id, request.NewEmail);

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}