using ApiSecuityServer.Model;
using MediatR;

namespace ApiSecuityServer.Hub.Application.Abstractions.Messaging;

public interface ICommand : IRequest<ApiResponse>
{
}

public interface ICommand<TResponse> : IRequest<ApiResponse<TResponse>>
{
}
