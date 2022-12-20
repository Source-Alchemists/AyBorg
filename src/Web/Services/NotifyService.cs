using System.Collections.Concurrent;
using AyBorg.SDK.Communication.gRPC;

namespace AyBorg.Web.Services;

public sealed class NotifyService : INotifyService
{
    private readonly ILogger<NotifyService> _logger;

    public ConcurrentBag<Subscription> Subscriptions { get; } =  new();

    public NotifyService(ILogger<NotifyService> logger)
    {
        _logger = logger;
    }

    public Subscription CreateSubscription(string ServiceUniqueName, NotifyType type)
    {
        var sub = new Subscription { ServiceUniqueName = ServiceUniqueName, Type = type };
        Subscriptions.Add(sub);
        return sub;
    }

    public void Unsubscribe(Subscription subscription)
    {
        var tmpSubs = new List<Subscription>();
        while (Subscriptions.TryTake(out Subscription? sub))
        {
            if (sub == subscription)
            {
                continue;
            }

            tmpSubs.Add(sub);
        }

        foreach (Subscription ts in tmpSubs)
        {
            Subscriptions.Add(ts);
        }
    }

    public record Subscription
    {
        public string ServiceUniqueName { get; init; } = string.Empty;
        public NotifyType Type { get; init; }
        public Action<object> Callback { get; set; } = null!;
    }
}
