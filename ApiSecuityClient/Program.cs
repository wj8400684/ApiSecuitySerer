using System.Text.Json.Serialization;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

var connection = new HubConnectionBuilder()
    .WithUrl("ws://localhost:5000/secuity")
    .AddJsonProtocol(o =>
    {
        o.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    })
    .Build();

await connection.StartAsync();

var sign = await connection.InvokeAsync<GdfpAtlasSignReplyMessage>("GdfpAtlasSign",
    new GdfpAtlasSignMessage(new byte[] { 1, 32, 32 }));

Console.WriteLine(sign);
Console.ReadKey();
