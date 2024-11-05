using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace ApiSecuityServer;

internal sealed class JsonHubProtocolOptionSetup : IConfigureOptions<JsonHubProtocolOptions>
{
    public void Configure(JsonHubProtocolOptions options)
    {
        options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    }
}