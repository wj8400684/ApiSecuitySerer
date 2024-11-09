namespace ApiSecuityServer.Hub.Hubs;

public sealed class ClientUserInfo
{
    public string UUID { get; set; }
    
    public int Platform { get; set; }

    public string NickName { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string? Host { get; set; }

    public int Port { get; set; }
}