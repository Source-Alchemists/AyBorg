using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.MQTT;

public abstract class BaseMqttStep : IStepBody
{
    protected readonly ILogger _logger;
    protected readonly IMqttClientProvider _mqttClientProvider = null!;
    protected readonly StringPort _topicPort = new("Topic", PortDirection.Input, $"AyBorg/results/{Guid.NewGuid()}");
    protected IList<IPort> _ports = new List<IPort>();

    public abstract string DefaultName { get; }

    public IEnumerable<IPort> Ports => _ports;

    public BaseMqttStep(ILogger logger, IMqttClientProvider mqttClientProvider)
    {
        _logger = logger;
        _mqttClientProvider = mqttClientProvider;
    }

    public abstract ValueTask<bool> TryRunAsync(CancellationToken cancellationToken);
}