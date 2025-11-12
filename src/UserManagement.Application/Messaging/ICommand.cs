using MediatR;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Messaging;

public interface ICommand<T> : IRequest<Result<T>>
{
}

public interface ICommand : IRequest<Result>
{
}