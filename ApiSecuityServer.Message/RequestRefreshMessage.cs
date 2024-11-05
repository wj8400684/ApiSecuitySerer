namespace ApiSecuityServer.Message;

public record struct RequestRefreshMessage(string Username);

public record struct RequestRefreshReplyMessage(bool IsSuccessful = true, string? ErrorMessage = default);