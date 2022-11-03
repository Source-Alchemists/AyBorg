using Atomy.SDK;
using Atomy.SDK.MQTT;
using Atomy.SDK.Ports;
using Microsoft.Extensions.Logging;

namespace Atomy.Plugins.Base.MQTT;

public abstract class BaseMqttStep : IStepBody
{
    protected readonly ILogger _logger;
    protected readonly IMqttClientProvider _mqttClientProvider = null!;
    protected readonly StringPort _topicPort = new StringPort("Topic", PortDirection.Input, $"atomy/results/{Guid.NewGuid()}");
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