using Ayborg.Gateway.Agent.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.Communication.gRPC;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class NotifyPassthroughServiceV1 : Notify.NotifyBase
{
    private readonly ILogger<NotifyPassthroughServiceV1> _logger;
    private readonly IGrpcChannelService _channelService;

    public NotifyPassthroughServiceV1(ILogger<NotifyPassthroughServiceV1> logger, IGrpcChannelService service)
    {
        _logger = logger;
        _channelService = service;
    }

    public override Task CreateStream(CreateNotifyStreamRequest request, IServerStreamWriter<NotifyMessage> responseStream, ServerCallContext context)
    {
        return Task.Factory.StartNew(async () =>
        {
            ChannelInfo channelInfo = _channelService.GetChannelByName(request.ServiceUniqueName);
            if (channelInfo == null)
            {
                _logger.LogWarning("Channel for {ServiceUniqueName} not found.", request.ServiceUniqueName);
                return;
            }

            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (!channelInfo.Notifications.TryDequeue(out Notification? cachecNotification))
                {
                    _logger.LogWarning("Notifications for {ServiceUniqueName} could not be dequeued.", request.ServiceUniqueName);
                    continue;
                }

                if (cachecNotification == null)
                {
                    _logger.LogWarning("Notification for {ServiceUniqueName} was null.", request.ServiceUniqueName);
                    continue;
                }

                await responseStream.WriteAsync(new NotifyMessage
                {
                    AgentUniqueName = cachecNotification.ServiceUniqueName,
                    Type = (int)cachecNotification.NotifyType,
                    Payload = cachecNotification.Payload
                });
            }
        });
    }

    public override Task<Empty> EngineIterationFinished(EngineIterationFinishedArgsDto request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName("AyBorg.Web");
            foreach (ChannelInfo channel in channels)
            {
                if(channel.Notifications.Count > 10)
                {
                    _ = channel.Notifications.TryDequeue(out _);
                }
                channel.Notifications.Enqueue(new Notification(
                request.AgentUniqueName,
                NotifyType.AgentIterationFinished,
                request.IterationId));
            }

            return new Empty();
        });
    }
}
