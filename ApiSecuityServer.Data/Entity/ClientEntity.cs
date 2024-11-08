namespace ApiSecuityServer.Data.Entity;

/// <summary>
/// 
/// </summary>
public sealed class ClientEntity
{
    /// <summary>
    /// 
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 硬盘id
    /// </summary>
    public string? DiskId { get; set; }

    /// <summary>
    /// 系统描述信息
    /// </summary>
    public required string SystemCaption { get; set; }

    /// <summary>
    /// 系统版本
    /// </summary>
    public required string SystemVersion { get; set; }

    /// <summary>
    /// 登录的系统账号
    /// </summary>
    public string? SystemUser { get; set; }

    /// <summary>
    /// 系统名称
    /// </summary>
    public required string SystemName { get; set; }

    /// <summary>
    /// mac地址
    /// </summary>
    public required string Mac { get; set; }

    /// <summary>
    /// cpu名称
    /// </summary>
    public required string CpuName { get; set; }

    /// <summary>
    /// 序列号
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public required string IpAddress { get; set; }

    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime RegisterTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后的在线时间
    /// </summary>
    public DateTime LastActiveTime { get; set; } = DateTime.UtcNow;
}
