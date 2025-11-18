using Microsoft.Extensions.Logging;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Get;

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, GetUserResponse>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetUserQueryHandler> _logger;

    public GetUserQueryHandler(
        IUserService userService,
        ILogger<GetUserQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<GetUserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        if (request.RequesterId is not null)
        {
            _logger.LogWarning("User with ID '{RequesterId}' requested information about user with ID {UserId}", request.RequesterId, request.UserId);
        }

        var user = await _userService.GetAsync(request.UserId);

        if (user.IsFailure) return user.Error;

        var response = new GetUserResponse(
            user.Value.Id,
            user.Value.FirstName,
            user.Value.LastName,
            user.Value.DateOfBirth,
            user.Value.Email,
            user.Value.Roles.Select(r => r.Name),
            user.Value.IsEmailVerified,
            user.Value.IsDeactivated
        );

        return response;
    }
}