using System.Text.Json.Serialization;

namespace ApiSecuityServer.Message;

[JsonSerializable(typeof(AtlasSignMessage))]
[JsonSerializable(typeof(AtlasSignReplyMessage))]
[JsonSerializable(typeof(GdfpAtlasSignReplyMessage))]
[JsonSerializable(typeof(GdfpAtlasSignMessage))]
[JsonSerializable(typeof(RequestRefreshMessage))]
[JsonSerializable(typeof(RequestRefreshReplyMessage))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}