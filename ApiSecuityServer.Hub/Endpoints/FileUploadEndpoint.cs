using ApiSecuityServer.Dtos;
using ApiSecuityServer.Hub;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using FastEndpoints;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Endpoints;

/// <summary>
/// 文件上传
/// </summary>
public sealed class FileUploadEndpoint(
    FileManger fileManger,
    ClientHubContainer container,
    IHubContext<ClientHub, IClientApi> clientContext,
    ILogger<FileDownloadEndpoint> logger) : Endpoint<FileUpdateRequest, ApiResponse<FileUpdateResultModel>>
{
    private readonly IHubClients<IClientApi> _clients = clientContext.Clients;

    public override void Configure()
    {
        AllowAnonymous();
        AllowFileUploads();
        Post("/api/file/upload");
    }

    public override async Task<ApiResponse<FileUpdateResultModel>> ExecuteAsync(FileUpdateRequest req,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ConnectionId))
            return ApiResponse.Fail<FileUpdateResultModel>("参数错误");

        var hubContext = container.GetById(req.ConnectionId);
        if (hubContext == null)
            return ApiResponse.Fail<FileUpdateResultModel>("客户端离线");

        var fileChannel = new FileChannel(
            connectionId: req.ConnectionId,
            fileName: req.Body.FileName,
            fileSize: req.Body.Length,
            clientContext: hubContext,
            fileManger: fileManger,
            clientApi: _clients.Client(req.ConnectionId));

        var result = fileChannel.AddClient();

        if (!result)
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");

        fileChannel.IniFile();

        try
        {
            await req.Body.CopyToAsync(fileChannel.Stream!, ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "上传失败请重试");
            fileChannel.RemoveClient();
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");
        }

        try
        {
            await fileChannel.PublishDownloadAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "推送到客户端失败");
            fileChannel.RemoveClient();
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");
        }

        fileChannel.Complete();

        return new FileUpdateResultModel(fileChannel.Id);
    }
}