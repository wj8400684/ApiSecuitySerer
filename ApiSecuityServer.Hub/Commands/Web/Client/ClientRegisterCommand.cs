using ApiSecuityServer.Model;
using MediatR;

namespace ApiSecuityServer.Hub.Commands.Web.Client;

internal readonly record struct ClientRegisterCommand(string ClientId, string ClientName, string CpuName, int Platform, string ProductId, string OsVersion) : IRequest<ApiResponse>;

internal sealed class ClientRegisterCommandHandler(ILogger<ClientRegisterCommandHandler> logger) : IRequestHandler<ClientRegisterCommand, ApiResponse>
{
    public Task<ApiResponse> Handle(ClientRegisterCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
