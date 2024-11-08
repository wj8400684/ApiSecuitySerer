using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Commands.Hub;

public readonly record struct ClientConnectionNotification(
    HubCallerContext Context,
    IGroupManager Group)
    : INotification;

internal sealed class ClientConnectionNotificationHandler(
    ILogger<ClientConnectionNotificationHandler> logger,
    IHubContext<ClientHub, IClientApi> hubContext,
    ClientHubContainer container) : INotificationHandler<ClientConnectionNotification>
{
    public async Task Handle(ClientConnectionNotification notification, CancellationToken cancellationToken)
    {
        var context = notification.Context;
        var group = notification.Group;

        var httpContext = context.GetHttpContext();

        if (httpContext == null)
        {
            context.Abort();
            return;
        }

        var request = httpContext.Request;

        var nickName = request.Query["nickName"];
        var groupName = request.Query["groupName"];
        var platform = request.Query["platform"];

        if (string.IsNullOrWhiteSpace(nickName)
            || string.IsNullOrWhiteSpace(groupName)
            || string.IsNullOrWhiteSpace(platform)
            || nickName.Count == 0
            || groupName.Count == 0)
        {
            context.Abort();
            return;
        }

        logger.LogInformation("{0} 用户连接", nickName!);
        
        var userInfo = new ClientUserInfo
        {
            NickName = nickName!,
            GroupName = groupName!,
            Platform = Convert.ToInt32(platform),
            Host = httpContext.Connection.RemoteIpAddress?.ToString(),
            Port = httpContext.Connection.RemotePort
        };
        
        context.Features.Set(userInfo);
        container.Add(context);

        await group.AddToGroupAsync(context.ConnectionId, groupName!, cancellationToken);

        var message = new ConnectionEventMessage(
            ConnectionId: context.ConnectionId,
            NickName: context.ConnectionId,
            Platform: 1,
            Host: "userInfo.Host",
            Port: userInfo.Port);

        //推送连接信息到
        await hubContext.Clients.GroupExcept(userInfo.GroupName, context.ConnectionId).PushConnectionEventAsync(message);

        container.CopyTo(out var hubCallerContexts);

        if (hubCallerContexts.Length <= 1)
            return;

        var connections = hubCallerContexts.Where(h => h.ConnectionId != context.ConnectionId).Select(h =>
        {
            var u = h.Features.Get<ClientUserInfo>()!;

            return new ConnectionEventMessage(
                ConnectionId: h.ConnectionId,
                NickName: u.NickName,
                Platform: u.Platform,
                Host: u.Host,
                Port: u.Port);
        });

        //同步其他客户端信息
        await hubContext.Clients.Client(context.ConnectionId).PublishSyncConnectionInfoAsync(new ConnectionInfoMessage(connections));
    }
}