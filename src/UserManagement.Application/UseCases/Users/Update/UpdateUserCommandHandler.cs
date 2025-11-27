using UserManagement.Application.Contexts;
using UserManagement.Application.Interfaces;
using UserManagement.Application.Messaging;
using UserManagement.Application.UseCases.Users.Get;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.UseCases.Users.Update;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, GetUserResponse>
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUserService userService,
        IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var context = new UpdateUserContext(request);

        var user = await _userService.UpdateAsync(context);

        if (user.IsFailure) return user.Error;

        await _unitOfWork.SaveChangesAsync();

        var response = new GetUserResponse(
            user: user.Value,
            isDeactivated: await _userService.IsDeacivated(user.Value.Id)
        );

        return response;
    }
}