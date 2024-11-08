using ApiSecuityServer;
using ApiSecuityServer.Data;
using ApiSecuityServer.Hub.Hubs;
using ApiSecuityServer.Hubs;
using FluentValidation;
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
builder.Services.AddSqliteEfCore(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);
builder.Services.AddSingleton<FileManger>();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("enableSwaggerApi"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapHub<ClientHub>("/api/chat");
app.Run();