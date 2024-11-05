using ApiSecuityServer.Dtos;
using ApiSecuityServer.Hub;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;
using FastEndpoints;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Endpoints;

/// <summary>
/// 文件上传
/// </summary>
public sealed class FileUpdateEndpoint(
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
            clientApi: _clients.Client(req.ConnectionId),
            clientContext: hubContext,
            fileManger: fileManger);

        var result = fileChannel.AddFile();

        if (!result)
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");

        fileChannel.IniFile();

        try
        {
            await fileChannel.PublishDownloadAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "推送到客户端失败");
            fileChannel.RemoveFile();
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");
        }

        try
        {
            await using var stream = req.Body.OpenReadStream();
            while (true)
            {
                var buffer = new byte[1024];
                var size = await stream.ReadAsync(buffer, ct);

                if (size == 0)
                    break;

                if (size < buffer.Length)
                    await fileChannel.WriterAsync(buffer.AsSpan()[..size].ToArray(), ct);
                else
                    await fileChannel.WriterAsync(buffer, ct);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "上传失败请重试");
            fileChannel.RemoveFile();
            return ApiResponse.Fail<FileUpdateResultModel>("上传失败请重试");
        }

        fileChannel.Complete();
        
        return new FileUpdateResultModel(fileChannel.Id);
    }
}