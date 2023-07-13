using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

internal sealed class DeviceManagerService : IDeviceManagerService
{
    private readonly ILogger<DeviceManagerService> _logger;
    private readonly IPluginsService _pluginsService;

    public IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders => _pluginsService.DeviceProviders;

    public DeviceManagerService(ILogger<DeviceManagerService> logger, IPluginsService pluginsService)
    {
        _logger = logger;
        _pluginsService = pluginsService;
    }

    public async ValueTask<IDeviceProxy> AddAsync(string providerName, string deviceId)
    {
        if (DeviceProviders.Any(p => p.Devices.Any(d => d.Id.Equals(deviceId))))
        {
            throw new ArgumentException($"Device '{deviceId}' already exists");
        }

        IDeviceProviderProxy? provider = DeviceProviders.FirstOrDefault(p => p.Name.Equals(providerName))
                                            ?? throw new KeyNotFoundException($"Device provider '{providerName}' not found");

        return await provider.AddAsync(deviceId);
    }

    public async ValueTask<IDeviceProxy> RemoveAsync(string deviceId)
    {
        IDeviceProviderProxy provider = DeviceProviders.First(p => p.Devices.Any(d => d.Id.Equals(deviceId)));
        return await provider.RemoveAsync(deviceId);
    }
}
