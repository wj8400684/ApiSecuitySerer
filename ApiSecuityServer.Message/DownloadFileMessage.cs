namespace ApiSecuityServer.Message;

/// <summary>
/// 
/// </summary>
/// <param name="FileId">文件id</param>
/// <param name="FileName">文件名称</param>
/// <param name="FileSize">文件大小</param>
/// <param name="PartNumber">文件需要合并文件需要</param>
public record struct DownloadFileMessage(string FileId, string FileName, long FileSize,int PartNumber);