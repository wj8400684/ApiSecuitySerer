namespace ApiSecuityServer.Dtos;

public sealed record FileDownloadRequest(string FileId);

public sealed record FileDownloadResultModel(string FileId);