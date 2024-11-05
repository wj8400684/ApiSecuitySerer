using System.Text.Json.Serialization;

namespace ApiSecuityServer.Message;

[JsonSerializable(typeof(PublishChatMessage))]
[JsonSerializable(typeof(PublishDownloadMessage))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}