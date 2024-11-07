
using FastEndpoints;

namespace ApiSecuityServer.Dtos;

public sealed class FileUploadChunkRequest
{
    /// <summary>
    /// 文件名
    /// </summary>
    [QueryParam]
    public string? FileName { get; set; }
    
    /// <summary>
    /// 当前分片
    /// </summary>
    
    [QueryParam]public int PartNumber { get; set; }

    private int _size;
    
    /// <summary>
    /// 缓冲区大小
    /// </summary>
    [QueryParam]
    public int Size
    {
        get { return _size; }

        set
        {
            if (_size > 8)
            {
                _size = 8;
            }
            _size = value;
        }
    }
    
    /// <summary>
    /// 分片总数
    /// </summary>
    [QueryParam]
    public int Chunks { get; set; }
    
    /// <summary>
    /// 文件读取起始位置
    /// </summary>
    [QueryParam]
    public int Start { get; set; }
    
    /// <summary>
    /// 文件读取结束位置
    /// </summary>
    [QueryParam]
    public int End { get; set; }
    
    /// <summary>
    /// 文件大小
    /// </summary>
    [QueryParam]
    public int Total { get; set; }
}

