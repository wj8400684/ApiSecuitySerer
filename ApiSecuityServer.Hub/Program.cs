using ApiSecuityServer;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR()
    .AddJsonProtocol()
    .AddContainer();

builder.Services.AddFastEndpoints()
    .AddSwaggerDocument();

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureAllOptions();
builder.Services.AddSingleton<FileManger>();
builder.Services.AddHostedService<MessageHubSendServer>();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("enableSwaggerApi"))
{
    app.UseSwaggerGen();
}

app.UseFastEndpoints();
app.MapHub<ClientHub>("/chat");
app.Run();