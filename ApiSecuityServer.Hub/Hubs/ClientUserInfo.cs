namespace ApiSecuityServer.Hub.Hubs;

public sealed class ClientUserInfo
{
    public int Platform { get; set; }

    public string NickName { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string? Host { get; set; }

    public int Port { get; set; }
}