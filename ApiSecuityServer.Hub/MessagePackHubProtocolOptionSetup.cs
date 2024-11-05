using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace ApiSecuityServer;

internal sealed class MessagePackHubProtocolOptionSetup : IConfigureOptions<MessagePackHubProtocolOptions>
{
    public void Configure(MessagePackHubProtocolOptions options)
    {
        //options.SerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);
    }
}