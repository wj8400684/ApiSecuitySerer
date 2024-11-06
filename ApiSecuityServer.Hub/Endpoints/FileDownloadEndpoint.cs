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

        var file = fileManger.GetFile(req.FileId);
        if (file == null)
        {
            await SendAsync(ApiResponse.Fail<FileDownloadResultModel>("文件不存在"), cancellation: ct);
            return;
        }

        try
        {
            file.Stream!.Position = 0;
            await SendStreamAsync(file.Stream!, cancellation: ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        finally
        {
            file.RemoveClient();
        }
    }
}