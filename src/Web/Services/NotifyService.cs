/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Concurrent;
using AyBorg.Communication.gRPC;

namespace AyBorg.Web.Services;

public sealed class NotifyService : INotifyService
{
    public ConcurrentBag<Subscription> Subscriptions { get; } =  new();

    public Subscription Subscribe(string ServiceUniqueName, NotifyType type)
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
