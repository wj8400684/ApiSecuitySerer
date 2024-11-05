namespace ApiSecuityServer.Message;

public record struct PublishDownloadMessage(string FileId, string FileName, long FileSize);