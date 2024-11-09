using System.Text.Json.Serialization;

namespace ApiSecuityServer.Model;

[JsonSerializable(typeof(ApiResponse))]
[JsonSerializable(typeof(ApiResponse<>))]
[JsonSerializable(typeof(ClientRegisterRequest))]
[JsonSerializable(typeof(FileUploadRequest))]
[JsonSerializable(typeof(FileUpdateResultModel))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}