using System.Text.Json;
using FastEndpoints;
using Microsoft.Extensions.Options;

namespace ApiSecuityServer.Hub;

public sealed class ConfigOptionSetup : IConfigureOptions<Config>
{
    public void Configure(Config options)
    {
        options.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Serializer.Options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    }
}