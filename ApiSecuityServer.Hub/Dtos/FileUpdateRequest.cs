namespace ApiSecuityServer.Dtos;

public sealed record FileUpdateRequest(string ConnectionId, int FileOffset, IFormFile Body);

public sealed record FileUpdateResultModel(string FileId);