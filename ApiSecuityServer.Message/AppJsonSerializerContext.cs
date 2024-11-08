using System.Text.Json.Serialization;

namespace ApiSecuityServer.Message;

[JsonSerializable(typeof(ChatMessage))]
[JsonSerializable(typeof(DownloadFileMessage))]
[JsonSerializable(typeof(ConnectionEventMessage))]
[JsonSerializable(typeof(ConnectionInfoMessage))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}