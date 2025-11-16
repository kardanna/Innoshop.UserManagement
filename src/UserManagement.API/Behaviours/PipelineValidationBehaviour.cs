using FluentValidation;
using MediatR;
using UserManagement.Domain.Shared;

namespace UserManagement.API.Behaviours;

public class PipelineValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<PipelineValidationBehaviour<TRequest, TResponse>> _logger;

    public PipelineValidationBehaviour(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<PipelineValidationBehaviour<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var errors = _validators
            .Select(v => v.Validate(request))
            .SelectMany(vr => vr.Errors)
            .Where(vf => vf is not null)
            .Select(vf => new Error(
                vf.ErrorCode,
                vf.ErrorMessage))
            .DistinctBy(e => new { e.Code, e.Description })
            .ToList();

        if (errors.Count == 0)
        {
            return await next();
        }
        
        _logger.LogError("A validation problem has occurred : {@Errors}", errors);

        return ToValidationResult<TResponse>(errors);
    }

    private static T ToValidationResult<T>(IEnumerable<Error> errors)
        where T : Result
    {
        if (typeof(T) == typeof(Result))
        {
            return (ValidationResult.WithErrors(errors.ToArray()) as T)!;
        }

        object result = typeof(ValidationResult<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(T)
                .GenericTypeArguments[0])
            .GetMethod(nameof(ValidationResult.WithErrors))!
            .Invoke(null, new object?[] { errors.ToArray() })!;

        return (T)result;
    }
}