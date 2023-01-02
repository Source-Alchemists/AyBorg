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

    public Action<NotifyMessage> DownstreamNotified { get; set; } = null!;

    public NotifyPassthroughServiceV1(ILogger<NotifyPassthroughServiceV1> logger, IGrpcChannelService service)
    {
        _logger = logger;
        _channelService = service;
    }

    public override Task CreateDownstream(CreateNotifyStreamRequest request, IServerStreamWriter<NotifyMessage> responseStream, ServerCallContext context)
    {
        return Task.Factory.StartNew(async () =>
        {
            ChannelInfo channelInfo = _channelService.GetChannelByName(request.ServiceUniqueName);
            if (channelInfo == null)
            {
                _logger.LogWarning("Channel for {ServiceUniqueName} not found.", request.ServiceUniqueName);
                return;
            }

            try
            {
                channelInfo.IsAcceptingNotifications = true;
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    if (!channelInfo.Notifications.TryTake(out Notification? cachecNotification, -1, context.CancellationToken))
                    {
                        continue;
                    }

                    if (cachecNotification == null)
                    {
                        _logger.LogWarning("Notification for {ServiceUniqueName} was null.", request.ServiceUniqueName);
                        continue;
                    }

                    var message = new NotifyMessage
                    {
                        AgentUniqueName = cachecNotification.ServiceUniqueName,
                        Type = (int)cachecNotification.NotifyType,
                        Payload = cachecNotification.Payload
                    };
                    await responseStream.WriteAsync(message);

                    DownstreamNotified?.Invoke(message);
                }
            }
            finally
            {
                if (channelInfo != null)
                {
                    channelInfo.IsAcceptingNotifications = false;
                    while (channelInfo.Notifications.TryTake(out _)) ;
                }
            }
        });
    }

    public override Task<Empty> CreateNotificationFromAgent(NotifyMessage request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName("AyBorg.Web");
            foreach (ChannelInfo channel in channels.Where(c => c.IsAcceptingNotifications))
            {
                while (channel.Notifications.Count >= 10)
                {
                    _ = channel.Notifications.TryTake(out _);
                }
                channel.Notifications.Add(new Notification(
                request.AgentUniqueName,
                (NotifyType)request.Type,
                request.Payload));
            }

            return new Empty();
        });
    }
}
