using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer;

internal sealed class MessageHubSendServer(IHubContext<ClientHub, IClientApi> clientContext) : BackgroundService
{
    private readonly IHubClients<IClientApi> _clients = clientContext.Clients;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
            
            try
            {
                //await _clients(new RequestRefreshMessage(Guid.NewGuid().ToString("N")));
            }
            catch (Exception e)
            {
                
            }
        }
    }
}