using ApiSecuityServer;
using ApiSecuityServer.Hubs;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSignalR()
    .AddJsonProtocol();

builder.Services.ConfigureOptions<JsonHubProtocolOptionSetup>();

var app = builder.Build();

app.MapHub<SecuityHub>("/secuity");

app.Run();