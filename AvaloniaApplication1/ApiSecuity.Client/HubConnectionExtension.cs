using System;
using System.Threading.Tasks;
using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR.Client;

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
}