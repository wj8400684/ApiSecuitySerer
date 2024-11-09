using ApiSecuityServer.Hub.Commands.Web.Client;
using ApiSecuityServer.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiSecuityServer.Controllers;

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
    public async ValueTask<ApiResponse> RegisterClientAsync([FromBody] ClientRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ClientRegisterCommand(
            Serial: request.Serial,
            OSName: request.OSName,
            OSVersion: request.OSVersion,
            Guid: request.Guid,
            UUID: request.UUID,
            Processor: request.Processor,
            Vendor: request.Vendor,
            HttpContext: HttpContext,
            Product: request.Product,
            Memory: request.Memory);

        return await mediator.Send(command, cancellationToken);
    }
}