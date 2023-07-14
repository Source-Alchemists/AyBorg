using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

internal sealed class DeviceProxyManagerService : IDeviceProxyManagerService
{
    private readonly ILogger<DeviceProxyManagerService> _logger;
    private readonly IPluginsService _pluginsService;

    public IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders => _pluginsService.DeviceProviders;

    public DeviceProxyManagerService(ILogger<DeviceProxyManagerService> logger, IPluginsService pluginsService)
    {
        _logger = logger;
        _pluginsService = pluginsService;
    }

    public async ValueTask<IDeviceProxy> AddAsync(AddDeviceOptions options)
    {
        if (DeviceProviders.Any(p => p.Devices.Any(d => d.Id.Equals(options.DeviceId))))
        {
            throw new ArgumentException($"Device '{options.DeviceId}' already exists");
        }

        IDeviceProviderProxy? provider = DeviceProviders.FirstOrDefault(p => p.Name.Equals(options.ProviderName))
                                            ?? throw new KeyNotFoundException($"Device provider '{options.ProviderName}' not found");

        return await provider.AddAsync(options);
    }

    public async ValueTask<IDeviceProxy> RemoveAsync(string deviceId)
    {
        IDeviceProviderProxy provider = DeviceProviders.First(p => p.Devices.Any(d => d.Id.Equals(deviceId)));
        return await provider.RemoveAsync(deviceId);
    }

    public async ValueTask<IDeviceProxy> ChangeStateAsync(ChangeDeviceStateOptions options)
    {
        IDeviceProxy device = DeviceProviders.Select(p => p.Devices.First(d => d.Id.Equals(options.DeviceId, StringComparison.InvariantCultureIgnoreCase))).Single();
        if (device.IsActive && device.IsConnected && options.Activate)
        {
            _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' is already active", options.DeviceId);
            return device;
        }

        if (!device.IsActive && !options.Activate)
        {
            _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' is already deactivated", options.DeviceId);
            return device;
        }

        if (options.Activate && !await device.TryConnectAsync())
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' failed to connect", options.DeviceId);
            return device;
        }

        if (!options.Activate && !await device.TryDisconnectAsync())
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' failed to disconnect", options.DeviceId);
            return device;
        }

        return device;
    }
}
