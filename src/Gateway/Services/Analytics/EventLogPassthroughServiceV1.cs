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

using Ayborg.Gateway.Analytics.V1;
using AyBorg.Communication;
using AyBorg.Gateway.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Analytics;

public sealed class EventLogPassthroughServiceV1 : EventLog.EventLogBase
{
    private readonly IGrpcChannelService _channelService;

    public EventLogPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<Empty> LogEvent(EventEntry request, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Log);
        foreach (ChannelInfo channel in channels)
        {
            EventLog.EventLogClient client = _channelService.CreateClient<EventLog.EventLogClient>(channel.ServiceUniqueName);
            await client.LogEventAsync(request);
        }

        return new Empty();
    }

    public override async Task GetLogEvents(GetEventsRequest request, IServerStreamWriter<EventEntry> responseStream, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Log);
        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            EventLog.EventLogClient client = _channelService.CreateClient<EventLog.EventLogClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<EventEntry> response = client.GetLogEvents(request, cancellationToken: context.CancellationToken);
            await foreach (EventEntry? entry in response.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(entry, cancellationToken: context.CancellationToken);
            }
        });
    }
}
