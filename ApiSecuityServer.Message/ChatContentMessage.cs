namespace ApiSecuityServer.Message;

public record struct ChatContentMessage(string SenderId, string SenderName, string Content);