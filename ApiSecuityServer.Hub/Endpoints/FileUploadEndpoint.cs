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

        var file = new FileTransferStream(
            id: hubContext.ConnectionId,
            fileName: req.Body.FileName,
            fileSize: req.Body.Length,
            fileManger: fileManger,
            clientApi: _clients.Client(req.ConnectionId));

        var result = file.Add();

        if (!result)
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");

        try
        {
            await file.Writer(req.Body, ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "上传失败请重试");
            file.Remove();
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");
        }

        try
        {
            await file.PublishDownloadAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "推送到客户端失败");
            file.Remove();
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");
        }

        return new FileUpdateResultModel(file.Id);
    }
}