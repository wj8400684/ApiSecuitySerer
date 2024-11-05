using ApiSecuityServer;
using ApiSecuityServer.Hub;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 1073741824000; //set to max allowed file size of your system
});

builder.Services.AddSignalR()
    .AddJsonProtocol()
    .AddContainer();

builder.Services.AddFastEndpoints()
    .SwaggerDocument();

builder.Services.AddSingleton<FileManger>();
builder.Services.AddHostedService<MessageHubSendServer>();
builder.Services.ConfigureOptions<ConfigOptionSetup>();
builder.Services.ConfigureOptions<JsonHubProtocolOptionSetup>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwaggerGen();

app.UseFastEndpoints();
app.MapHub<ClientHub>("/chat");
app.Run();