using ApiSecuityServer.Hub;
using ApiSecuityServer.Message;

namespace ApiSecuityServer.Hubs;

internal sealed partial class ClientHub
{
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [HubClientApiCommand(ServerClientApiCommand.SendMessage)]
    public async Task<InvokerResultMessage> OnSendMessageAsync(ChatContentMessage message)
    {
        return InvokerResultMessage.Successful();
    }
}