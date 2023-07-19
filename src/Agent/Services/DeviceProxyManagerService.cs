using AyBorg.Data.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

internal sealed class DeviceProxyManagerService : IDeviceProxyManagerService
{
    private readonly ILogger<DeviceProxyManagerService> _logger;
    private readonly IPluginsService _pluginsService;
    private readonly IDeviceToStorageMapper _storageMapper;
    private readonly IDeviceRepository _deviceRepository;

    public IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders => _pluginsService.DeviceProviders;

    public DeviceProxyManagerService(ILogger<DeviceProxyManagerService> logger,
                                        IPluginsService pluginsService,
                                        IDeviceToStorageMapper storageMapper,
                                        IDeviceRepository deviceRepository)
    {
        _logger = logger;
        _pluginsService = pluginsService;
        _storageMapper = storageMapper;
        _deviceRepository = deviceRepository;
    }

    public async void Load()
    {
        IEnumerable<DeviceRecord> deviceRecords = await _deviceRepository.GetAllAsync();
    }

    public async ValueTask<IDeviceProxy> AddAsync(AddDeviceOptions options)
    {
        if (DeviceProviders.Any(p => p.Devices.Any(d => d.Id.Equals(options.DeviceId))))
        {
            throw new ArgumentException($"Device '{options.DeviceId}' already exists");
        }

        IDeviceProviderProxy? provider = DeviceProviders.FirstOrDefault(p => p.Name.Equals(options.ProviderName))
                                            ?? throw new KeyNotFoundException($"Device provider '{options.ProviderName}' not found");

        IDeviceProxy newDevice = await provider.AddAsync(options);

        DeviceRecord deviceRecord = _storageMapper.Map(newDevice);
        try
        {
            await _deviceRepository.AddAsync(deviceRecord);
        }
        catch (Exception)
        {
            // Rollback
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed to add device '{deviceId}' to storage, removing from provider", newDevice.Id);
            await provider.RemoveAsync(newDevice.Id);
            throw;
        }
        return newDevice;
    }

    public async ValueTask<IDeviceProxy> RemoveAsync(string deviceId)
    {
        IDeviceProviderProxy provider = DeviceProviders.First(p => p.Devices.Any(d => d.Id.Equals(deviceId)));
        IDeviceProxy removedDevice = await provider.RemoveAsync(deviceId);

        DeviceRecord deviceRecord = _storageMapper.Map(removedDevice);
        await _deviceRepository.RemoveAsync(deviceRecord);

        return removedDevice;
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

        DeviceRecord deviceRecord = _storageMapper.Map(device);
        await _deviceRepository.UpdateAsync(deviceRecord);

        return device;
    }
}
