using ApiSecuityServer.Message;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hub;

internal sealed class HubClientApiCommand(ServerClientApiCommand command) : HubMethodNameAttribute(((int)command).ToString())
{
}