namespace ApiSecuityServer.Data.Entity;

/// <summary>
/// 
/// </summary>
public sealed class ClientEntity
{
    /// <summary>
    ///设备id
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 系统名称
    /// </summary>
    public required string OsName { get; set; }

    /// <summary>
    /// 系统版本
    /// </summary>
    public required string OsVersion { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? Product { get; set; }

    /// <summary>
    /// 设备生产者
    /// </summary>
    public string? Vendor { get; set; }

    /// <summary>
    /// 处理器信息
    /// </summary>
    public string? Processor { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? Guid { get; set; }

    /// <summary>
    /// 内存
    /// </summary>
    public long Memory { get; set; }

    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime RegisterTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后的在线时间
    /// </summary>
    public DateTime LastActiveTime { get; set; } = DateTime.UtcNow;
}