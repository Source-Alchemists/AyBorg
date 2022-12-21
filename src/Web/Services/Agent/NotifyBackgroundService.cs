using System.Text.Json;
using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.System.Configuration;
using AyBorg.SDK.System.Runtime;
using static AyBorg.Web.Services.NotifyService;

namespace AyBorg.Web.Services;

public sealed class NotifyBackgroundService : BackgroundService
{
    private readonly ILogger<NotifyBackgroundService> _logger;
    private readonly Notify.NotifyClient _notifyClient;
    private readonly INotifyService _notifyService;
    private readonly IServiceConfiguration _serviceConfiguration;

    public NotifyBackgroundService(ILogger<NotifyBackgroundService> logger, Notify.NotifyClient notifyClient, INotifyService notifyService, IServiceConfiguration serviceConfiguration)
    {
        _logger = logger;
        _notifyClient = notifyClient;
        _notifyService = notifyService;
        _serviceConfiguration = serviceConfiguration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            Grpc.Core.AsyncServerStreamingCall<NotifyMessage> response = _notifyClient.CreateDownstream(new CreateNotifyStreamRequest { ServiceUniqueName = _serviceConfiguration.UniqueName }, cancellationToken: stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await response.ResponseStream.MoveNext(stoppingToken))
                {
                    NotifyMessage notifyMessage = response.ResponseStream.Current;
                    var notifyType = (NotifyType)notifyMessage.Type;
                    IEnumerable<Subscription> matchingSubscriptions = _notifyService.Subscriptions.Where(s => s.ServiceUniqueName.Equals(notifyMessage.AgentUniqueName, StringComparison.InvariantCultureIgnoreCase)
                                                                                            && s.Type.Equals(notifyType));

                    object callbackObject = null!;
                    switch ((NotifyType)notifyMessage.Type)
                    {
                        case NotifyType.AgentEngineStateChanged:
                            EngineMeta? engineMeta = JsonSerializer.Deserialize<EngineMeta>(notifyMessage.Payload);
                            callbackObject = engineMeta!;
                            break;
                        case NotifyType.AgentIterationFinished:
                            callbackObject = Guid.Parse(notifyMessage.Payload);
                            break;
                        default:
                            break;
                    }

                    if (callbackObject != null)
                    {
                        foreach (Subscription sub in matchingSubscriptions)
                        {
                            sub.Callback?.Invoke(callbackObject);
                        }
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }, TaskCreationOptions.LongRunning);
    }
}
