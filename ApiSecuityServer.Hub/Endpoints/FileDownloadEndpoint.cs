using ApiSecuityServer.Dtos;
using ApiSecuityServer.Hub;
using FastEndpoints;

namespace ApiSecuityServer.Endpoints;

/// <summary>
/// 文件下载
/// </summary>
/// <param name="fileManger"></param>
/// <param name="logger"></param>
public sealed class FileDownloadEndpoint(
    FileManger fileManger,
    ILogger<FileDownloadEndpoint> logger)
    : Endpoint<FileDownloadRequest>
{
    public override void Configure()
    {
        AllowAnonymous();
        Get("api/file/download");
    }

    public override async Task HandleAsync(FileDownloadRequest req,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FileId))
        {
            await SendAsync(ApiResponse.Fail<FileDownloadResultModel>("参数错误"), cancellation: ct);
            return;
        }

        var fileChannel = fileManger.GetChannel(req.FileId);
        if (fileChannel == null)
        {
            await SendAsync(ApiResponse.Fail<FileDownloadResultModel>("文件不存在"), cancellation: ct);
            return;
        }

        try
        {
            using var str = new MemoryStream();
            await foreach (var m in fileChannel.ReadAsync(ct))
                await str.WriteAsync(m, ct);
            await SendStreamAsync(str, cancellation: ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        finally
        {
            fileChannel.RemoveFile();
        }
    }
}