using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.System.Configuration;

namespace AyBorg.Web.Services;

public sealed class NotifyService : BackgroundService, INotifyService
{
    private readonly ILogger<NotifyService> _logger;
    private readonly Notify.NotifyClient _notifyClient;
    private readonly IServiceConfiguration _serviceConfiguration;

    public Action<Guid> AgentIterationFinished { get; set; } = null!;

    public NotifyService(ILogger<NotifyService> logger, Notify.NotifyClient notifyClient, IServiceConfiguration serviceConfiguration)
    {
        _logger = logger;
        _notifyClient = notifyClient;
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
                    Console.WriteLine(notifyMessage.ToString());
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }, TaskCreationOptions.LongRunning);
    }
}
