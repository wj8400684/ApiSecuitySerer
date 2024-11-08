using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Commands.Hub;

public readonly record struct ClientDisconnectionNotification(HubCallerContext Context, IGroupManager Group)
    : INotification;

internal sealed class ClientDisconnectionNotificationHandler(
    ILogger<ClientDisconnectionNotificationHandler> logger,
    IHubContext<ClientHub, IClientApi> hubContext,
    ClientHubContainer container) : INotificationHandler<ClientDisconnectionNotification>
{
    public async Task Handle(ClientDisconnectionNotification notification, CancellationToken cancellationToken)
    {
        var context = notification.Context;
        var group = notification.Group;

        container.Remove(context.ConnectionId);
        
        var userInfo = context.Features.Get<ClientUserInfo>();

        if (userInfo == null)
            return;

        logger.LogInformation("{0} 断开连接", userInfo.NickName);

        await group.RemoveFromGroupAsync(context.ConnectionId, userInfo.GroupName, cancellationToken);

        context.Features.Set<ClientUserInfo>(null);

        var message = new ConnectionEventMessage(
            ConnectionId: context.ConnectionId,
            NickName: userInfo.NickName,
            Platform: userInfo.Platform,
            Host: userInfo.Host,
            Port: userInfo.Port);

        await hubContext.Clients.Group(userInfo.GroupName).PushDisConnectionEventAsync(message);
    }
}