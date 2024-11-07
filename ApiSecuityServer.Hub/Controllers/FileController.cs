using ApiSecuityServer.Commands;
using ApiSecuityServer.Dtos;
using ApiSecuityServer.Hub.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiSecuityServer.Controllers;

[Route("api/file")]
public sealed class FileController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("upload")]
    [DisableFormValueModelBinding]
    public async ValueTask<ApiResponse<FileUpdateResultModel>> UploadFileAsync(
        [FromQuery] FileUploadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new FileUploadCommand(
            HttpContext: HttpContext,
            FileName: request.FileName,
            PartNumber: request.PartNumber,
            Chunks: request.Chunks,
            Start: request.Start,
            End: request.End,
            Total: request.Total);

        return await mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("delete")]
    public async ValueTask<ApiResponse> DeleteFileAsync([FromQuery] FileDeleteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new FileDeleteCommand(request.FileId);
        return await mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    [HttpPost]
    [Route("download")]
    public async ValueTask DownFileAsync([FromQuery] FileDownloadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new FileDownloadCommand();
        await mediator.Publish(command, cancellationToken);
    }
}