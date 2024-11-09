using ApiSecuityServer.Data.Entity;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Commands.Hub;

internal struct HubConnectionRequest
{
    public HubConnectionRequest(HttpContext httpContext)
    {
        NickName = httpContext.Request.Query["nickname"];
        GroupName = httpContext.Request.Query["groupName"];
        UUID = httpContext.Request.Query["uuid"];

        if (int.TryParse(httpContext.Request.Query["platform"], out var platform))
            Platform = platform;
    }

    public string? NickName { get; set; }

    public string? GroupName { get; set; }

    public int Platform { get; set; }

    public string? UUID { get; set; }

    public bool Check()
    {
        return !string.IsNullOrWhiteSpace(NickName) &&
               !string.IsNullOrWhiteSpace(GroupName) &&
               !string.IsNullOrWhiteSpace(UUID);
    }
}

internal readonly record struct ClientConnectionNotification(
    HubCallerContext Context,
    IGroupManager Group)
    : INotification
{
    public HubConnectionRequest GetHubConnectionRequest()
    {
        return new HubConnectionRequest(Context.GetHttpContext()!);
    }
}

internal sealed class ClientConnectionNotificationHandler(
    IUnitOfWork unitOfWork,
    ILogger<ClientConnectionNotificationHandler> logger,
    IHubContext<ClientHub, IClientApi> hubContext,
    ClientHubContainer container) : INotificationHandler<ClientConnectionNotification>
{
    private readonly IRepository<ClientEntity> _repository = unitOfWork.Repository<ClientEntity>();

    public async Task Handle(ClientConnectionNotification notification, CancellationToken cancellationToken)
    {
        #region 获取参数

        var context = notification.Context;
        var group = notification.Group;

        var httpContext = context.GetHttpContext();

        if (httpContext == null)
        {
            context.Abort();
            return;
        }

        var request = notification.GetHubConnectionRequest();

        if (request.Check())
        {
            context.Abort();
            return;
        }

        #endregion

        logger.LogInformation("{0} 用户连接", request.NickName);

        #region 从数据库获取客户端信息

        var client =
            await _repository.SingleOrDefaultAsync(_repository.SingleResultQuery().AndFilter(c => c.Id == request.UUID),
                cancellationToken); //获取客户端信息

        if (client == null) //没有注册关闭连接
        {
            context.Abort();
            return;
        }

        var result = await _repository.UpdateAsync(c => c.Id == request.UUID,
            p => p.SetProperty(c => c.LastActiveTime, newVlue => DateTime.Now), cancellationToken);

        logger.LogInformation(result > 0 ? "{0} 更新用户信息成功" : "{0} 更新用户信息失败", request.NickName);

        #endregion

        #region 添加客户端信息

        var userInfo = new ClientUserInfo
        {
            UUID = request.UUID!,
            NickName = request.NickName!,
            GroupName = request.GroupName!,
            Platform = request.Platform,
            Host = httpContext.Connection.RemoteIpAddress?.ToString(),
            Port = httpContext.Connection.RemotePort
        };

        context.Features.Set(userInfo);
        container.Add(context);

        #endregion

        #region 推送当前连接到群中

        await group.AddToGroupAsync(context.ConnectionId, request.GroupName!, cancellationToken);

        var message = new ConnectionEventMessage(
            ConnectionId: context.ConnectionId,
            NickName: userInfo.NickName,
            Platform: 1,
            Host: "userInfo.Host",
            Port: userInfo.Port);

        //推送连接信息到
        await hubContext.Clients.GroupExcept(userInfo.GroupName, context.ConnectionId)
            .PushConnectionEventAsync(message);

        #endregion

        #region 推送其他客户端信息

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
        await hubContext.Clients.Client(context.ConnectionId)
            .PublishSyncConnectionInfoAsync(new ConnectionInfoMessage(connections));

        #endregion
    }
}