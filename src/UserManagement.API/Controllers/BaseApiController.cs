using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Domain.Shared;

namespace UserManagement.API.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected readonly ISender _sender;

    protected BaseApiController(ISender sender)
    {
        _sender = sender;
    }

    protected IActionResult HandleFailure(Result result)
    {
        return result switch
        {
            { IsSuccess: true }
                => throw new InvalidOperationException("Can not handle success result."),

            IValidationResult vr
                => BadRequest(ToProblemDetails(
                    "Validation Error",
                    StatusCodes.Status400BadRequest,
                    result.Error,
                    vr.Errors)),

            _ => BadRequest(ToProblemDetails(
                    "Bad Request",
                    StatusCodes.Status400BadRequest,
                    result.Error))
        };
    }

    private static ProblemDetails ToProblemDetails(
        string title,
        int status,
        Error error,
        Error[]? errors = null
    )
    {
        var pd = new ProblemDetails()
        {
            Title = title,
            Status = status,
            Type = error.Code,
            Detail = error.Description
        };

        if (errors != null)
        {
            pd.Extensions.Add(nameof(errors), errors);
        }

        return pd;
    }
}