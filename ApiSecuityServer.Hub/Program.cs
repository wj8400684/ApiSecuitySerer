using ApiSecuityServer;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR()
    .AddJsonProtocol()
    .AddContainer();

builder.Services.ConfigureAllOptions();
builder.Services.AddSingleton<FileManger>();
builder.Services.AddHostedService<MessageHubSendServer>();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("enableSwaggerApi"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<ClientHub>("/chat");
app.Run();