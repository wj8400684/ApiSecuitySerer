using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hub.Hubs;


internal static class HubExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverBuilder"></param>
    /// <returns></returns>
    public static ISignalRServerBuilder AddContainer(this ISignalRServerBuilder serverBuilder)
    {
        serverBuilder.Services.AddSingleton<ClientHubContainer>();
        
        return serverBuilder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverBuilder"></param>
    /// <returns></returns>
    public static ISignalRServerBuilder AddRequestTimeoutCancellationToken(this ISignalRServerBuilder serverBuilder)
    {
        //serverBuilder.Services.AddSingleton<ITimeoutCancellationToken, HubRequestTimeoutCancellationToken>();
        
        return serverBuilder;
    }
    
    /// <summary>
    /// 客户端是否在线
    /// </summary>
    /// <param name="container"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IsOnline(this ClientHubContainer container, string id)
    {
        return container.GetById(id) != null;
    }
}