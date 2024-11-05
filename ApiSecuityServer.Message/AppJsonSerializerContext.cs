using System.Text.Json.Serialization;

namespace ApiSecuityServer.Message;

[JsonSerializable(typeof(AtlasSignMessage))]
[JsonSerializable(typeof(AtlasSignReplyMessage))]
[JsonSerializable(typeof(GdfpAtlasSignReplyMessage))]
[JsonSerializable(typeof(GdfpAtlasSignMessage))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}