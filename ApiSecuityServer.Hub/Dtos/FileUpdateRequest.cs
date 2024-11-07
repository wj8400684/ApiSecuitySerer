namespace ApiSecuityServer.Dtos;

/// <summary>
/// 
/// </summary>
/// <param name="FileName">文件名</param>
/// <param name="PartNumber">序号</param>
/// <param name="Chunks">分块大小</param>
/// <param name="Start">开始位置</param>
/// <param name="End">结束</param>
/// <param name="Total">文件大小</param>
public sealed record FileUploadRequest(
    string ConnectionId,
    string FileName,
    int PartNumber,
    int Chunks,
    int Start,
    int Size,
    int End,
    int Total);

public sealed record FileUpdateResultModel(string FileId);