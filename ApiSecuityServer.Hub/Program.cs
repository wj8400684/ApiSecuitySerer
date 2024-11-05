using ApiSecuityServer;
using ApiSecuityServer.Hubs;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSignalR()
                .AddMessagePackProtocol();

builder.Services.AddHostedService<MessageHubSendServer>();
builder.Services.ConfigureOptions<MessagePackHubProtocolOptionSetup>();

var app = builder.Build();

app.MapHub<ClientHub>("/order");

app.Run();