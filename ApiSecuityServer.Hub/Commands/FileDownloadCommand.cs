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
    private static readonly ReadOnlyMemory<char> EndMark = "-".ToCharArray();

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

        if (request.HttpContext.Request.Headers.Range.Count == 0)
        {
            await request.WriteErrorAsync(ApiResponse.Error("请求参数不正确"), 404, cancellationToken);
            return;
        }

        var range = request.HttpContext.Request.Headers.Range.ToString().AsMemory();

        var reader = new SequenceReader<char>(new ReadOnlySequence<char>(range));

        if (!reader.TryAdvanceTo('='))
        {
            await request.WriteErrorAsync(ApiResponse.Error("请求参数不正确"), 404, cancellationToken);
            return;
        }

        if (!reader.TryReadTo(out ReadOnlySequence<char> startSequence, EndMark.Span, advancePastDelimiter: true))
        {
            await request.WriteErrorAsync(ApiResponse.Error("请求参数不正确"), 404, cancellationToken);
            return;
        }

        if (!int.TryParse(startSequence.ToString(), out var start) ||
            !int.TryParse(reader.UnreadSequence.ToString(), out var end))
        {
            await request.WriteErrorAsync(ApiResponse.Error("请求参数不正确"), 404, cancellationToken);
            return;
        }

        var re = file.ChannelStream1!.Reader;

        try
        {
            while (await re.WaitToReadAsync(cancellationToken))
            {
                if (!re.TryRead(out var stream))
                    continue;

                var totalWriterLength = 0;
                var readerLength = end - start;
                var buffer = ArrayPool<byte>.Shared.Rent(readerLength);

                while (true)
                {
                    var remainingLength = (int)(stream.Length - stream.Position); //剩余长度

                    if (remainingLength == 0)
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    readerLength = end - start; //需要的长度

                    if (totalWriterLength == readerLength) //完成
                        break;

                    var size = await stream.ReadAsync(buffer, cancellationToken);

                    if (size < buffer.Length)
                    {
                        await request.HttpContext.Response.BodyWriter.WriteAsync(buffer.AsMemory(0, size),
                            cancellationToken);
                    }
                    else
                    {
                        await request.HttpContext.Response.BodyWriter.WriteAsync(buffer, cancellationToken);
                    }

                    totalWriterLength += size;
                }

                ArrayPool<byte>.Shared.Return(buffer);
            }
            
            await request.HttpContext.Response.BodyWriter.CompleteAsync();
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