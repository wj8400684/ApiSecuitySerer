using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

var connection = new HubConnectionBuilder()
    .WithUrl("ws://193.112.192.177:6767/secuity")
    .AddJsonProtocol(s =>
    {
        s.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    })
    .Build();

await connection.StartAsync();

for (var i = 0; i < 100000000; i++)
{
    var sign = await connection.InvokeAsync<GdfpAtlasSignReplyMessage>("GdfpAtlasSign",
        new GdfpAtlasSignMessage(new byte[] { 1, 32, 32 }));

    Console.WriteLine(sign);
}


// .AddJsonProtocol(o =>
// {
//     o.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
// })
Console.ReadKey();