using AyBorg.SDK.Common;
using AyBorg.SDK.Communication.MQTT;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.MQTT;

public sealed class MqttClientProvider : IDeviceProvider
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MqttClientProvider> _logger;
    private readonly IMqttClientProviderFactory _clientProviderFactory;

    public string Name => "MQTT Clients";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { "Communication", "MQTT" };

    public bool CanCreate => true;

    public MqttClientProvider(ILogger<MqttClientProvider> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _clientProviderFactory = new MqttClientProviderFactory();
    }

    public MqttClientProvider(ILogger<MqttClientProvider> logger, ILoggerFactory loggerFactory, IMqttClientProviderFactory clientProviderFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _clientProviderFactory = clientProviderFactory;
    }

    public async ValueTask<IDevice> CreateAsync(string id)
    {
        var client = new MqttClient(_loggerFactory.CreateLogger<MqttClient>(), _clientProviderFactory, id);
        _logger.LogInformation((int)EventLogType.Plugin, "Added MQTT client '{id}'", id);
        return await ValueTask.FromResult(client);
    }
}
