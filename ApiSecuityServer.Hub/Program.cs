using ApiSecuityServer.Hubs;
using ApiSecuityServer.Message;

var builder = WebApplication.CreateSlimBuilder(args); 

builder.Services.AddSignalR()
    .AddJsonProtocol(o =>
    {
        o.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });

var app = builder.Build();

app.MapHub<SecuityHub>("/secuity");

app.Run();


