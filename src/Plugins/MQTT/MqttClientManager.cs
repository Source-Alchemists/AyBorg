using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Communication.MQTT;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.MQTT;

public sealed class MqttClientManager : IDeviceManager
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MqttClientManager> _logger;
    private readonly IMqttClientProviderFactory _clientProviderFactory;
    private ImmutableList<IDevice> _devices = ImmutableList.Create<IDevice>();

    public string Name => "MQTT Clients";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { "Communication", "MQTT" };

    public bool CanAdd => true;

    public bool CanRemove => true;

    public IReadOnlyCollection<IDevice> Devices => _devices;

    public MqttClientManager(ILogger<MqttClientManager> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _clientProviderFactory = new MqttClientProviderFactory();
    }

    public MqttClientManager(ILogger<MqttClientManager> logger, ILoggerFactory loggerFactory, IMqttClientProviderFactory clientProviderFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _clientProviderFactory = clientProviderFactory;
    }

    public async ValueTask<IDevice> AddAsync(string id)
    {
        if (Devices.Any(d => d.Name.Equals(id, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new ArgumentException($"Device with name '{id}' already exists", nameof(id));
        }

        var client = new MqttClient(_loggerFactory.CreateLogger<MqttClient>(), _clientProviderFactory, id);
        _devices = _devices.Add(client);
        _logger.LogInformation((int)EventLogType.Plugin, "Added MQTT client '{id}'", id);
        return await ValueTask.FromResult(client);
    }

    public async ValueTask<IDevice> RemoveAsync(string id)
    {
        IDevice? client = _devices.FirstOrDefault(d => d.Name.Equals(id, StringComparison.InvariantCultureIgnoreCase)) ?? throw new ArgumentException($"Device with name '{id}' does not exist", nameof(id));
        _devices = _devices.Remove(client);
        if (client is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _logger.LogInformation((int)EventLogType.Plugin, "Removed MQTT client '{id}'", id);
        return await ValueTask.FromResult(client);
    }
}
