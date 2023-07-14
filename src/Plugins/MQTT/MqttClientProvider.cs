using AyBorg.SDK.Common;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.MQTT;

public sealed class MqttClientProvider : IDeviceProvider
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MqttClientProvider> _logger;
    private readonly ICommunicationStateProvider _communicationStateProvider;
    private readonly IMqttClientProviderFactory _clientProviderFactory;

    public string Name => "MQTT Clients";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { "Communication", "MQTT" };

    public bool CanCreate => true;

    public MqttClientProvider(ILogger<MqttClientProvider> logger, ILoggerFactory loggerFactory, ICommunicationStateProvider communicationStateProvider)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _communicationStateProvider = communicationStateProvider;
        _clientProviderFactory = new MqttClientProviderFactory();
    }

    public MqttClientProvider(ILogger<MqttClientProvider> logger, ILoggerFactory loggerFactory, ICommunicationStateProvider communicationStateProvider, IMqttClientProviderFactory clientProviderFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _communicationStateProvider = communicationStateProvider;
        _clientProviderFactory = clientProviderFactory;
    }

    public async ValueTask<IDevice> CreateAsync(string id)
    {
        var client = new MqttClient(_loggerFactory.CreateLogger<MqttClient>(), _clientProviderFactory, _communicationStateProvider, id);
        _logger.LogTrace((int)EventLogType.Plugin, "Added MQTT client '{id}'", id);
        return await ValueTask.FromResult(client);
    }
}
