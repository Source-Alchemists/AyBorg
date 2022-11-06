using Autodroid.SDK.Common.Ports;
using Microsoft.Extensions.Logging;
using Autodroid.SDK.Communication.MQTT;
using Autodroid.SDK.Common;

namespace Autodroid.Plugins.Base.MQTT;

public abstract class BaseMqttStep : IStepBody
{
    protected readonly ILogger _logger;
    protected readonly IMqttClientProvider _mqttClientProvider = null!;
    protected readonly StringPort _topicPort = new("Topic", PortDirection.Input, $"Autodroid/results/{Guid.NewGuid()}");
    protected IList<IPort> _ports = new List<IPort>();

    public abstract string DefaultName { get; }

    public IEnumerable<IPort> Ports => _ports;

    public BaseMqttStep(ILogger logger, IMqttClientProvider mqttClientProvider)
    {
        _logger = logger;
        _mqttClientProvider = mqttClientProvider;
    }

    public abstract Task<bool> TryRunAsync(CancellationToken cancellationToken);
}