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
    [HubClientApiCommand(ServerClientApiCommand.DownloadFiles)]
    Task PublishDownloadFileAsync(DownloadFileMessage message);    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubClientApiCommand(ServerClientApiCommand.ConnectionInfo)]
    Task PublishConnectionInfoAsync(ConnectionInfoMessage message);
}