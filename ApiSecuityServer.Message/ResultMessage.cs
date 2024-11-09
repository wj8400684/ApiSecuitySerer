namespace ApiSecuityServer.Message;

public record struct InvokerResultMessage(bool IsSuccessful, string? ErrorMessage = default)
{
    public static InvokerResultMessage Successful() => new(true);

    public static InvokerResultMessage Error(string msg) => new(false, msg);
}