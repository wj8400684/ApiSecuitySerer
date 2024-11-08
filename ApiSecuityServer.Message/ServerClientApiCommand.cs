namespace ApiSecuityServer.Message;

public enum ServerClientApiCommand
{
    /// <summary>
    /// 同步连接信息
    /// </summary>
    SyncConnectionInfo,
    /// <summary>
    /// 推送连接事件
    /// </summary>
    PushConnectionEvent,
    /// <summary>
    /// 推送下载数据通知
    /// </summary>
    PushDownloadDataEvent,
    /// <summary>
    /// 推送断开连接通知
    /// </summary>
    PushDisConnectionEvent,
}