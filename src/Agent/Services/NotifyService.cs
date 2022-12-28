using System.Text.Json;
using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Communication.gRPC.Models;
using AyBorg.SDK.System.Configuration;
using AyBorg.SDK.System.Runtime;

namespace AyBorg.Agent.Services;

public sealed class NotifyService : INotifyService
{
    private readonly ILogger<NotifyService> _logger;
    private readonly IServiceConfiguration _serviceConfiguration;
    private readonly Notify.NotifyClient _notifyClient;

    public NotifyService(ILogger<NotifyService> logger, IServiceConfiguration serviceConfiguration, Notify.NotifyClient notifyClient)
    {
        _logger = logger;
        _serviceConfiguration = serviceConfiguration;
        _notifyClient = notifyClient;
    }

    public async ValueTask SendEngineStateAsync(EngineMeta engineMeta)
    {
        await _notifyClient.CreateNotificationFromAgentAsync(new NotifyMessage
        {
            AgentUniqueName = _serviceConfiguration.UniqueName,
            Type = (int)NotifyType.AgentEngineStateChanged,
            Payload = JsonSerializer.Serialize(engineMeta)
        });
    }

    public async ValueTask SendIterationFinishedAsync(Guid iterationId)
    {
        await _notifyClient.CreateNotificationFromAgentAsync(new NotifyMessage
        {
            AgentUniqueName = _serviceConfiguration.UniqueName,
            Type = (int)NotifyType.AgentIterationFinished,
            Payload = iterationId.ToString()
        });
    }

    public async ValueTask SendAutomationFlowChangedAsync(AgentAutomationFlowChangeArgs args)
    {
        await _notifyClient.CreateNotificationFromAgentAsync(new NotifyMessage
        {
            AgentUniqueName = _serviceConfiguration.UniqueName,
            Type = (int)NotifyType.AgentAutomationFlowChanged,
            Payload = JsonSerializer.Serialize(args)
        });
    }
}
