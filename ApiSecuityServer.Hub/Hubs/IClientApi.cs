using ApiSecuityServer.Hub;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace ApiSecuityServer.Hubs;

public interface IClientApi
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubClientApiCommand(ServerClientApiCommand.PushDownloadDataEvent)]
    Task PublishDownloadFileAsync(DownloadFileMessage message);    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubClientApiCommand(ServerClientApiCommand.SyncConnectionInfo)]
    Task PublishSyncConnectionInfoAsync(ConnectionInfoMessage message);
    
    /// <summary>
    /// 推送连接事件
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubClientApiCommand(ServerClientApiCommand.PushConnectionEvent)]
    Task PushConnectionEventAsync(ConnectionEventMessage message);

    /// <summary>
    /// 推送断开事件
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubClientApiCommand(ServerClientApiCommand.PushDisConnectionEvent)]
    Task PushDisConnectionEventAsync(ConnectionEventMessage message);
}