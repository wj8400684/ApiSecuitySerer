using ApiSecuityServer;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR()
    .AddJsonProtocol()
    .AddContainer();

builder.Services.AddMediatR();
builder.Services.AddValidators();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureAllOptions();
builder.Services.AddAndSetOptionsControllers();
builder.Services.AddSingleton<FileManger>();
builder.Services.AddHostedService<MessageHubSendServer>();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("enableSwaggerApi"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapHub<ClientHub>("/chat");
app.Run();