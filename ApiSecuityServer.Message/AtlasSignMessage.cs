namespace ApiSecuityServer.Message;

public record struct AtlasSignMessage(string Url, string Sign);

public record struct AtlasSignReplyMessage(string Sign, bool IsSuccessful = false, string? ErrorMessage = default);