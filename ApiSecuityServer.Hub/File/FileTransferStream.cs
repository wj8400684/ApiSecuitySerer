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
    IClientApi clientApi,
    FileManger fileManger,
    MultipartReader multipartReader)
{
    public long Size { get; } = fileSize;

    public string Id { get; } = Guid.NewGuid().ToString();

    public string Name { get; } = fileName;

    public string ConnectionId { get; set; } = id;

    public Channel<byte[]> ChannelStream { get; private set; } = Channel.CreateUnbounded<byte[]>();

    public async ValueTask ReadFileAsync(CancellationToken cancellationToken)
    {
        var writer = ChannelStream.Writer;

        while (true)
        {
            var section = await multipartReader.ReadNextSectionAsync(cancellationToken);

            if (section == null)
                break;

            while (true)
            {
                var buffer = new byte[bufferSize];
                var readSize = await section.Body.ReadAsync(buffer, cancellationToken);

                if (readSize == 0)
                    break;

                if (readSize == bufferSize)
                    await writer.WriteAsync(buffer, cancellationToken);
                else
                    await writer.WriteAsync(buffer.AsSpan(0, readSize).ToArray(), cancellationToken);
            }
        }
    }

    /// <summary>
    /// 读取完成
    /// </summary>
    public void ReadComplete()
    {
        ChannelStream.Writer.Complete();
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