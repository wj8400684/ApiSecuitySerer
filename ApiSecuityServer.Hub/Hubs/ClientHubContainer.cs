using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace ApiSecuityServer.Hub.Hubs;

public sealed class ClientHubContainer
{
    private readonly ConcurrentDictionary<string, HubCallerContext> _concurrent = new();

    public void Add(HubCallerContext context)
    {
        _concurrent.TryAdd(context.ConnectionId, context);
    }

    public HubCallerContext? GetById(string id)
    {
        _concurrent.TryGetValue(id, out var context);

        return context;
    }

    public IEnumerable<HubCallerContext> GetSessions(Predicate<HubCallerContext>? criteria = null)
    {
        using var enumerator = _concurrent.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var s = enumerator.Current.Value;

            if (criteria == null || criteria(s))
                yield return s;
        }
    }

    public void Remove(string clientId)
    {
        _concurrent.TryRemove(clientId, out var context);
    }
}