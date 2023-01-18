using System.Collections.Concurrent;
using AyBorg.SDK.Communication.gRPC;
using static AyBorg.Web.Services.NotifyService;

namespace AyBorg.Web.Services;

public interface INotifyService
{
    ConcurrentBag<Subscription> Subscriptions { get; }

    Subscription Subscribe(string ServiceUniqueName, NotifyType type);

    void Unsubscribe(Subscription subscription);
}
