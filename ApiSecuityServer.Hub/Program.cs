using ApiSecuityServer;
using ApiSecuityServer.Hubs;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSignalR()
                .AddMessagePackProtocol();

builder.Services.AddHostedService<MessageHubSendServer>();
builder.Services.ConfigureOptions<JsonHubProtocolOptionSetup>();

var app = builder.Build();

app.MapHub<ClientHub>("/order");

app.Run();