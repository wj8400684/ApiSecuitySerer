using System;
using System.Threading.Tasks;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSecuity.Client;

public static class HubConnectionExtension
{
    public static HubConnection RegisterHandler<TMessage>(this HubConnection connection, ServerClientApiCommand command,
        Action<TMessage> handler)
    {
        connection.On(((int)command).ToString(), handler);
        return connection;
    }

    public static HubConnection RegisterHandler<TMessage>(this HubConnection connection, ServerClientApiCommand command,
        Func<TMessage, Task> handler)
    {
        connection.On(((int)command).ToString(), handler);
        return connection;
    }

    public static HubConnection RegisterHandler<TMessage, TResultMessage>(this HubConnection connection,
        ServerClientApiCommand command,
        Func<TMessage, Task<TResultMessage>> handler)
    {
        connection.On(methodName: ((int)command).ToString(),
            handler: handler);

        return connection;
    }
   
    public static  TBuilder ConfigureJsonHubOptions<TBuilder>(this TBuilder builder) where TBuilder : ISignalRBuilder
    {
        builder.Services.Configure<JsonHubProtocolOptions>(s =>
        {
            s.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });
        
        return builder;
    }
}