using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hubs;

public sealed class ClientHub(ILogger<ClientHub> logger) : Hub<IClientApi>
{
    public override Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected {0}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client disconnected {0}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    // [HubMethodName("AtlasSign")]
    // public async Task<string> AtlasSignAsync(string url, string sign)
    // {
    //     await Task.CompletedTask;
    //     return Guid.NewGuid().ToString();
    // }
    //
    // [HubMethodName("GdfpAtlasSign")]
    // public async Task<GdfpAtlasSignReplyMessage> GdfpAtlasSignAsync(GdfpAtlasSignMessage message)
    // {
    //     await Task.CompletedTask;
    //     return new GdfpAtlasSignReplyMessage(Guid.NewGuid().ToString(), true);
    // }
}