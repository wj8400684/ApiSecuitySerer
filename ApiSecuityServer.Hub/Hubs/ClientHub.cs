using ApiSecuityServer.Commands.Hub;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hubs;

internal sealed partial class ClientHub(IMediator mediator) : Hub<IClientApi>
{
    public override async Task OnConnectedAsync()
    {
        var notification = new ClientConnectionNotification(Context, Groups);
        await mediator.Publish(notification, Context.ConnectionAborted);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var notification = new ClientDisconnectionNotification(Context, Groups);
        await mediator.Publish(notification, Context.ConnectionAborted);
    }
}