using ApiSecuityServer.Controllers;
using MediatR;

namespace ApiSecuityServer.Commands;

internal readonly record struct FileDownloadCommand(ControllerBase Controller, string FileId)
    : IRequest;

internal sealed class FileDownloadCommandHandler(
    FileManger fileManger,
    ILogger<FileDownloadCommandHandler> logger)
    : IRequestHandler<FileDownloadCommand>
{
    public async Task Handle(FileDownloadCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileId))
        {
            request.Controller.BadRequest($"Parameter '{nameof(request.FileId)}' cannot be null or empty.");
            return;
        }

        var file = fileManger.GetFile(request.FileId);
        if (file?.Stream == null)
        {
            request.Controller.BadRequest($"Parameter '{nameof(request.FileId)}' cannot be null or empty.");
            return;
        }

        if (request.Controller.Request.Headers.Range.Count == 0)
        {
            request.Controller.BadRequest($"Parameter '{nameof(request.FileId)}' cannot be null or empty.");
            return;
        }

        var s = request.Controller.Request.Headers.Range;

        try
        {
            
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        finally
        {
            file.Remove();
        }
    }
}