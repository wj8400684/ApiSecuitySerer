using ApiSecuityServer.Hub;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hubs;

public interface IClientApi
{
    /// <summary>
    /// 刷新所有
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubClientApiCommand(ServerClientApiCommand.RefreshAll)]
    Task RefreshAllAsync(RequestRefreshMessage message);

    /// <summary>
    /// 刷新指定
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubClientApiCommand(ServerClientApiCommand.RequestRefresh)]
    Task<RequestRefreshReplyMessage> RequestRefreshAsync(RequestRefreshMessage message);
}