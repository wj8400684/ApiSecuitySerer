using System.Threading.Channels;
using ApiSecuityServer.Hub;
using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;

namespace ApiSecuityServer;

public sealed class FileTransferStream(
    string id,
    string fileName,
    long fileSize,
    IClientApi clientApi,
    FileManger fileManger)
{
    public long Size { get; } = fileSize;

    public string Id { get; } = Guid.NewGuid().ToString();

    public string Name { get; } = fileName;

    public string ConnectionId { get; set; } = id;
    
    public MemoryStream? Stream { get; set; }

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