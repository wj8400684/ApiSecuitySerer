namespace ApiSecuityServer.Message;

public record struct GdfpAtlasSignMessage(byte[] Data);

public record struct GdfpAtlasSignReplyMessage(string Sign, bool IsSuccessful = false, string? ErrorMessage = default);