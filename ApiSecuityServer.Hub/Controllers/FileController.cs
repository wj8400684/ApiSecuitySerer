using ApiSecuityServer.Commands;
using ApiSecuityServer.Dtos;
using ApiSecuityServer.Hub.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ApiSecuityServer.Controllers;

[Route("api/file")]
public sealed class FileController(IMediator mediator, IOptions<JsonOptions> options) : ControllerBase
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
    /// <param name="fileId"></param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [Route("download/{fileId:guid}")]
    public async ValueTask DownFileAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var command = new FileDownloadCommand(options.Value, HttpContext, fileId.ToString());
        await mediator.Send(command, cancellationToken);
    }
}