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

    public void Start()
    {
        ChannelStream = Channel.CreateUnbounded<byte[]>();
    }

    public async ValueTask WriterAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[stream.Length];
        _ = await stream.ReadAsync(buffer, cancellationToken);
        await ChannelStream!.Writer.WriteAsync(buffer, cancellationToken);
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