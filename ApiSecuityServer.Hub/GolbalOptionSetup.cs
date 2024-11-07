using ApiSecuityServer.Message;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace ApiSecuityServer;

internal sealed class KestrelOptionsSetup : IConfigureOptions<KestrelServerOptions>
{
    public void Configure(KestrelServerOptions options)
    {
        options.Limits.MaxRequestBodySize = 10L * 1024 * 1024 * 1024; // 10GB
    }
}

internal sealed class FormOptionsSetup : IConfigureOptions<FormOptions>
{
    public void Configure(FormOptions options)
    {
        options.MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024; // 10GB
    }
}

internal sealed class JsonHubProtocolOptionSetup : IConfigureOptions<JsonHubProtocolOptions>
{
    public void Configure(JsonHubProtocolOptions options)
    {
        options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    }
}

internal static class OptionsSetupExtensions
{
    public static void ConfigureAllOptions(this IServiceCollection services)
    {
        services.ConfigureOptions<FormOptionsSetup>();
        services.ConfigureOptions<KestrelOptionsSetup>();
        services.ConfigureOptions<JsonHubProtocolOptionSetup>();
    }
}