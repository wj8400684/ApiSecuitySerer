using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hubs;

public sealed class SecuityHub : Hub
{
    [HubMethodName("SendMessage")]
    public async Task SendMessage(string username, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", username, message);
    }

    [HubMethodName("AtlasSign")]
    public async Task<string> AtlasSignAsync(string url, string sign)
    {
        await Task.CompletedTask;
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubMethodName("GdfpAtlasSign")]
    public async Task<GdfpAtlasSignReplyMessage> GdfpAtlasSignAsync(GdfpAtlasSignMessage message)
    {
        await Task.CompletedTask;
        return new GdfpAtlasSignReplyMessage(Guid.NewGuid().ToString(), true);
    }
}