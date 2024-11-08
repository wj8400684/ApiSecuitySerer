namespace ApiSecuity.Client.Model;

public sealed class ConnectionModel
{
    public string Id { get; set; } = null!;

    public string? Host { get; set; } 

    public int Port { get; set; } 
    
    public string NickName { get; set; } = null!;

    public int Platform { get; set; } = 1;
}