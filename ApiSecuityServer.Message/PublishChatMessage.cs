namespace ApiSecuityServer.Message;

public record struct PublishChatMessage(string SenderId, string SenderName, string Content);