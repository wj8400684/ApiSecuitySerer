using ApiSecuityServer.Hub;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace ApiSecuityServer.Hubs;

public interface IClientApi
{
    [HubClientApiCommand(ServerClientApiCommand.PublishDownload)]
    Task PublishDownloadFileAsync(PublishDownloadMessage message);    
}