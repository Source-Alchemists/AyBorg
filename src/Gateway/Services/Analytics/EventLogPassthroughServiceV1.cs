using Ayborg.Gateway.Analytics.V1;
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
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName("AyBorg.Analytics");
        foreach (ChannelInfo channel in channels)
        {
            EventLog.EventLogClient client = _channelService.CreateClient<EventLog.EventLogClient>(channel.ServiceUniqueName);
            await client.LogEventAsync(request);
        }

        return new Empty();
    }

    public override async Task GetLogEvents(GetEventsRequest request, IServerStreamWriter<EventEntry> responseStream, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName("AyBorg.Analytics");
        CancellationToken token = context.CancellationToken;
        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            EventLog.EventLogClient client = _channelService.CreateClient<EventLog.EventLogClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<EventEntry> response = client.GetLogEvents(request, cancellationToken: token);
            await foreach (EventEntry? entry in response.ResponseStream.ReadAllAsync(cancellationToken: token))
            {
                await responseStream.WriteAsync(entry, cancellationToken: token);
            }
        });
    }
}
