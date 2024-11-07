using System.Buffers;
using System.Buffers.Binary;
using System.Text.Json;
using ApiSecuityServer.Dtos;
using MediatR;

namespace ApiSecuityServer.Commands;

internal readonly record struct FileDownloadCommand(
    Microsoft.AspNetCore.Mvc.JsonOptions Options,
    HttpContext HttpContext,
    string FileId)
    : IRequest
{
    private readonly JsonSerializerOptions _serializerOptions = Options.JsonSerializerOptions;

    public async ValueTask WriteErrorAsync(ApiResponse response, int statusCode, CancellationToken cancellationToken)
    {
        HttpContext.Response.StatusCode = statusCode;
        await HttpContext.Response.WriteAsJsonAsync(response, _serializerOptions, cancellationToken: cancellationToken);
    }

    public async ValueTask WriteAsync(ApiResponse response, CancellationToken cancellationToken)
    {
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: cancellationToken);
    }
}

internal sealed class FileDownloadCommandHandler(
    FileManger fileManger,
    ILogger<FileDownloadCommandHandler> logger)
    : IRequestHandler<FileDownloadCommand>
{
    public async Task Handle(FileDownloadCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileId))
        {
            await request.WriteErrorAsync(ApiResponse.Error("文件不存在"), 404, cancellationToken);
            return;
        }

        var file = fileManger.GetFile(request.FileId);
        if (file?.ChannelStream == null)
        {
            await request.WriteErrorAsync(ApiResponse.Error("文件不存在"), 404, cancellationToken);
            return;
        }

        var reader = file.ChannelStream.Reader;

        try
        {
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                if (reader.TryRead(out var buffer))
                    await request.HttpContext.Response.BodyWriter.WriteAsync(buffer, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        finally
        {
            file.Remove();
            await request.HttpContext.Response.BodyWriter.CompleteAsync();
        }
    }
}