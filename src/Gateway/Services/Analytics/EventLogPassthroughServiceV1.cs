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

    public override async Task<Empty> LogEvent(EventRequest request, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName("AyBorg.Analytics");
        foreach (ChannelInfo channel in channels)
        {
            EventLog.EventLogClient client = _channelService.CreateClient<EventLog.EventLogClient>(channel.ServiceUniqueName);
            await client.LogEventAsync(request);
        }

        return new Empty();
    }
}
