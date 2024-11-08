using ApiSecuityServer.Hub.Commands.Web.Client;
using ApiSecuityServer.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiSecuityServer.Hub.Controllers;

[Route("api/client")]
public sealed class ClientController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// 注册设备信息
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("register")]
    public async ValueTask<ApiResponse> RegisterClientAsync([FromBody] ClientRegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new ClientRegisterCommand(
            ClientId: request.ClientId, 
            ClientName: request.ClientName, 
            CpuName: request.CpuName, 
            Platform: request.Platform, 
            ProductId: request.ProductId, 
            OsVersion: request.OsVersion);

        return await mediator.Send(command, cancellationToken);
    }
}
