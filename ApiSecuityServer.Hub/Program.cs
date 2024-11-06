using ApiSecuityServer;
using ApiSecuityServer.Hub;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// 配置 Kestrel 服务器以允许大文件上传
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10L * 1024 * 1024 * 1024; // 10GB
});
 
// 配置 FormOptions
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024; // 10GB
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

//if (app.Environment.IsDevelopment())
app.UseSwaggerGen();
app.UseFastEndpoints();
app.MapHub<ClientHub>("/chat");
app.Run();