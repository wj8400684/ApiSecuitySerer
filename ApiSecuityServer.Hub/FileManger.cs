using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hub;

public sealed class FileChannel(
    string connectionId,
    string fileName,
    long fileSize,
    IClientApi clientApi,
    FileManger fileManger,
    HubCallerContext clientContext)
{
    public long Size { get; } = fileSize;

    public string Id { get; } = Guid.NewGuid().ToString();

    public string Name { get; } = fileName;

    public string ConnectionId { get; private set; } = connectionId;

    public Channel<byte[]>? Stream { get; private set; }

    public void IniFile()
    {
        Stream = Channel.CreateUnbounded<byte[]>();
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    public async ValueTask WriterAsync(byte[] data, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(Stream, "还未初始化");

        await Stream.Writer.WriteAsync(data, cancellationToken);
    }

    /// <summary>
    /// 完成
    /// </summary>
    public void Complete()
    {
        ArgumentNullException.ThrowIfNull(Stream, "还未初始化");
        
        Stream.Writer.Complete();
    }

    /// <summary>
    /// 读取文件
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<byte[]> ReadAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(Stream, "还未初始化");

        return Stream.Reader.ReadAllAsync(cancellationToken);
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
    public bool AddFile()
    {
        if (!fileManger.AddFile(this))
            return false;

        clientContext.Items.Add(Id, this);
        return true;
    }

    /// <summary>
    /// 从当前客户端集合中删除文件
    /// </summary>
    public void RemoveFile()
    {
        fileManger.Delete(Id);
        clientContext.Items.Remove(Id);
    }
}

public sealed class FileManger
{
    private readonly ConcurrentDictionary<string, FileChannel> _files = new();

    /// <summary>
    /// 查找文件是否存在
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public bool Find(string fileId)
    {
        return _files.TryGetValue(fileId, out _);
    }

    /// <summary>
    /// 添加文件
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public bool AddFile(FileChannel channel)
    {
        return _files.TryAdd(channel.Id, channel);
    }

    /// <summary>
    /// 获取文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public FileChannel? GetChannel(string fileId)
    {
        _files.TryGetValue(fileId, out var channel);
        return channel;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fileId"></param>
    public void Delete(string fileId)
    {
        _files.TryRemove(fileId, out var _);
    }
}