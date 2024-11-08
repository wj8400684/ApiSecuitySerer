namespace ApiSecuityServer.Message;

public record struct ChatMessage(string SenderId, string SenderName, string Content);