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
    IClientApi clientApi,
    FileManger fileManger,
    MultipartReader multipartReader)
{
    public long Size { get; } = fileSize;

    public string Id { get; } = "08dc57cf-4ea4-4757-85f7-09ba2b463a99"; //Guid.NewGuid().ToString();

    public string Name { get; } = fileName;

    public string ConnectionId { get; set; } = id;

    public MemoryStream? Stream { get; set; }

    public Channel<byte[]>? ChannelStream { get; set; }

    public Channel<MemoryStream>? ChannelStream1 { get; set; }

    public void Start()
    {
        ChannelStream1 = Channel.CreateUnbounded<MemoryStream>();
        ChannelStream = Channel.CreateUnbounded<byte[]>();
    }

    public async ValueTask WriterAsync(Stream stream, CancellationToken cancellationToken)
    {
        var fileStream = new MemoryStream();
        await stream.CopyToAsync(stream, cancellationToken);
        fileStream.Position = 0;
        await ChannelStream1!.Writer.WriteAsync(fileStream, cancellationToken);

        return;

        ArgumentNullException.ThrowIfNull(ChannelStream);

        while (true)
        {
            var buffer = new byte[8196];
            var size = await stream.ReadAsync(buffer, cancellationToken);

            if (size == 0)
                break;

            if (size < buffer.Length)
                await ChannelStream.Writer.WriteAsync(buffer.AsSpan()[..size].ToArray(), cancellationToken);
            else
                await ChannelStream.Writer.WriteAsync(buffer, cancellationToken);
        }
    }

    public async ValueTask Writer(IFormFile file, CancellationToken cancellationToken)
    {
        Stream = new MemoryStream();
        await file.CopyToAsync(Stream, cancellationToken);
        Stream.Position = 0;
    }

    /// <summary>
    /// 推送文件下载通知到客户端
    /// </summary>
    public async ValueTask PublishDownloadAsync()
    {
        await clientApi.PublishDownloadFileAsync(new PublishDownloadMessage(Id, Name, Size));
    }

    /// <summary>
    /// 将这个文件添加到当前客户端集合中
    /// </summary>
    public bool Add()
    {
        return fileManger.AddFile(this);
    }

    /// <summary>
    /// 从当前客户端集合中删除文件
    /// </summary>
    public void Remove()
    {
        fileManger.DeleteAsync(Id);
    }
}