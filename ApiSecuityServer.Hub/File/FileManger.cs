using System.Collections.Concurrent;

namespace ApiSecuityServer;

public sealed class FileManger
{
    private readonly ConcurrentDictionary<string, FileTransferStream> _files = new();

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
    /// 获取文件
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    public IEnumerable<FileTransferStream> GetFiles(Predicate<FileTransferStream>? criteria = null)
    {
        using var enumerator = _files.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var s = enumerator.Current.Value;

            if (criteria == null || criteria(s))
                yield return s;
        }
    }

    /// <summary>
    /// 添加文件
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public bool AddFile(FileTransferStream file)
    {
        return _files.TryAdd(file.Id, file);
    }

    /// <summary>
    /// 获取文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public FileTransferStream? GetFile(string fileId)
    {
        _files.TryGetValue(fileId, out var channel);
        return channel;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fileId"></param>
    public ValueTask<bool> DeleteAsync(string fileId)
    {
        var result = _files.TryRemove(fileId, out var _);

        return ValueTask.FromResult(result);
    }
}