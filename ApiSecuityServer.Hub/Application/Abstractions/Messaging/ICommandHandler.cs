using ApiSecuityServer.Model;
using MediatR;

namespace ApiSecuityServer.Hub.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, ApiResponse>
    where TCommand : ICommand 
{
}

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, ApiResponse<TResponse>>
    where TCommand : ICommand<TResponse>
{
}