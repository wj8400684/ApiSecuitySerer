namespace ApiSecuityServer.Message;

public record struct ConnectionEventMessage(string ConnectionId, string NickName, string? Host, int Port, int Platform);

public record struct ConnectionInfoMessage(IEnumerable<ConnectionEventMessage> Clients);