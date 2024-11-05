using ApiSecuityServer.Hub.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hubs;

public sealed class ClientHub(ClientHubContainer container, ILogger<ClientHub> logger) : Hub<IClientApi>
{
    public override Task OnConnectedAsync()
    {
        container.Add(Context);
        logger.LogInformation("Client connected {0}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        container.Remove(Context.ConnectionId);
        logger.LogInformation("Client disconnected {0}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}