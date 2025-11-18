using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.API.DTOs;
using UserManagement.Application.UseCases.EmailAddresses.Change;
using UserManagement.Application.UseCases.EmailAddresses.Verify;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Domain.Shared;
using UserManagement.Domain.Errors;

namespace UserManagement.API.Controllers;

[Route("user/email")]
public class EmailController : BaseApiController
{
    private readonly ILogger<AuthController> _logger;

    public EmailController(
        ILogger<AuthController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

 
    [HttpPost("verify")]
    public async Task<IActionResult> Post([FromBody] VerifyEmailRequest request)
    {
        var command = new VerifyEmailAddressCommand(request.VerificationCode);

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
    
    [Authorize]
    [HttpPost("change")]
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
        
        var command = new ChangeEmailAddressCommand(id, request.NewEmail);

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}