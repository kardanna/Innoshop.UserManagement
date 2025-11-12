using MediatR;
using UserManagement.Domain.Shared;

namespace UserManagement.Application.Messaging;

public interface IQuery<T> : IRequest<Result<T>>
{
}