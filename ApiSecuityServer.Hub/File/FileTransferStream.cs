using System.Runtime.CompilerServices;
using System.Threading.Channels;
using ApiSecuityServer.Hub;
using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.WebUtilities;

namespace ApiSecuityServer;

public sealed class FileTransferStream(
    string id,
    string fileName,
    long fileSize,
    int bufferSize,
    int partNumber,
    IClientApi clientApi,
    MultipartReader multipartReader)
{
    private readonly Channel<Memory<byte>> _fileStream = Channel.CreateUnbounded<Memory<byte>>();

    public long Size { get; } = fileSize;

    public string Id { get; } = Guid.NewGuid().ToString();

    public string Name { get; } = fileName;

    public long Length => _fileStream.Reader.Count;

    public string ConnectionId { get; set; } = id;

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async ValueTask WriterAsync(CancellationToken cancellationToken)
    {
        var writer = _fileStream.Writer;

        while (await multipartReader.ReadNextSectionAsync(cancellationToken) is { } section)
        {
            int readSize;
            var buffer = new byte[bufferSize].AsMemory();
            while ((readSize = await section.Body.ReadAsync(buffer, cancellationToken)) != 0)
            {
                if (readSize == bufferSize)
                    await writer.WriteAsync(buffer, cancellationToken);
                else
                    await writer.WriteAsync(buffer[..readSize], cancellationToken);
                
                buffer = new byte[bufferSize].AsMemory();
            }
        }
    }

    /// <summary>
    /// 读取完成
    /// </summary>
    public void ReadComplete()
    {
        _fileStream.Writer.Complete();
    }

    /// <summary>
    /// 读取文件
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<Memory<byte>> ReadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var reader = _fileStream.Reader;

        while (await reader.WaitToReadAsync(cancellationToken))
        {
            if (reader.TryRead(out var buffer))
                yield return buffer;
        }
    }

    /// <summary>
    /// 推送文件下载通知到客户端
    /// </summary>
    public async ValueTask PublishDownloadAsync()
    {
        await clientApi.PublishDownloadFileAsync(new PublishDownloadMessage(Id, Name, Size, partNumber));
    }
}