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
    private readonly IRuntimeConverterService _runtimeConverter;
    public event EventHandler<ObjectChangedEventArgs>? DeviceChanged;
    public event EventHandler<CollectionChangedEventArgs>? DeviceCollectionChanged;
    public IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders => _pluginsService.DeviceProviders;

    public DeviceProxyManagerService(ILogger<DeviceProxyManagerService> logger,
                                        IPluginsService pluginsService,
                                        IDeviceToStorageMapper storageMapper,
                                        IDeviceRepository deviceRepository,
                                        IRuntimeConverterService runtimeConverter)
    {
        _logger = logger;
        _pluginsService = pluginsService;
        _storageMapper = storageMapper;
        _deviceRepository = deviceRepository;
        _runtimeConverter = runtimeConverter;
    }

    public async ValueTask LoadAsync()
    {
        var devicesAdded = new List<IDevice>();
        foreach (DeviceRecord deviceRecord in await _deviceRepository.GetAllAsync())
        {
            try
            {
                IDeviceProviderProxy? providerProxy = _pluginsService.FindDeviceProvider(deviceRecord.ProviderMetaInfo);
                if (providerProxy == null)
                {
                    _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Device provider '{providerName}' not found", deviceRecord.ProviderMetaInfo.TypeName);
                    continue;
                }

                IDeviceProxy? existingDevice = providerProxy.Devices.FirstOrDefault(d => d.Id.Equals(deviceRecord.Id, StringComparison.InvariantCultureIgnoreCase));
                if (existingDevice == null || !await TryLoadDevice(existingDevice, deviceRecord))
                {
                    _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed to load device '{deviceId}'", deviceRecord.Id);
                    continue;
                }

                if (!providerProxy.CanAdd)
                {
                    _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Device provider '{providerName}' does not support adding devices", deviceRecord.ProviderMetaInfo.TypeName);
                    continue;
                }

                IDeviceProxy device = await providerProxy.AddAsync(new AddDeviceOptions(string.Empty, deviceRecord.Id, deviceRecord.IsActive));

                if (!await TryLoadDevice(device, deviceRecord))
                {
                    _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed to load device '{deviceId}'", device.Id);
                    continue;
                }

                devicesAdded.Add(device.Native);

                _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' loaded from storage", device.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed to load device provider '{providerName}' for device '{deviceId}'", deviceRecord.ProviderMetaInfo.TypeName, deviceRecord.Id);
            }
        }

        DeviceCollectionChanged?.Invoke(this, new CollectionChangedEventArgs(DeviceProviders.SelectMany(p => p.Devices).Where(d => d.IsActive).Select(d => d.Native).ToList(), devicesAdded, Array.Empty<object>()));
    }

    public async ValueTask<IDeviceProxy> AddAsync(AddDeviceOptions options)
    {
        IDeviceProviderProxy? provider = DeviceProviders.FirstOrDefault(p => p.Name.Equals(options.ProviderName))
                                            ?? throw new KeyNotFoundException($"Device provider '{options.ProviderName}' not found");

        if (DeviceProviders.Any(p => p.Devices.Any(d => d.Id.Equals(options.DeviceId))))
        {
            throw new ArgumentException($"Device '{options.DeviceId}' already exists");
        }

        IDeviceProxy newDevice = await provider.AddAsync(options);

        DeviceRecord deviceRecord = _storageMapper.Map(newDevice);
        try
        {
            await _deviceRepository.AddAsync(deviceRecord);
            DeviceCollectionChanged?.Invoke(this, new CollectionChangedEventArgs(DeviceProviders.SelectMany(p => p.Devices).Where(d => d.IsActive).Select(d => d.Native).ToList(), new List<IDevice> { newDevice.Native }, Array.Empty<object>()));
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
        DeviceCollectionChanged?.Invoke(this, new CollectionChangedEventArgs(DeviceProviders.SelectMany(p => p.Devices).Where(d => d.IsActive).Select(d => d.Native).ToList(), Array.Empty<IDevice>(), new List<IDevice> { removedDevice.Native }));

        return removedDevice;
    }

    public async ValueTask<IDeviceProxy> ChangeStateAsync(ChangeDeviceStateOptions options)
    {
        IDeviceProviderProxy deviceProvider = DeviceProviders.First(p => p.Devices.Any(d => d.Id.Equals(options.DeviceId, StringComparison.InvariantCultureIgnoreCase)));
        IDeviceProxy device = deviceProvider.Devices.Single(d => d.Id.Equals(options.DeviceId, StringComparison.InvariantCultureIgnoreCase));
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

        if (options.Activate)
        {
            _logger.LogInformation(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' connected", options.DeviceId);
        }
        else
        {
            _logger.LogInformation(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' disconnected", options.DeviceId);

        }

        DeviceCollectionChanged?.Invoke(this, new CollectionChangedEventArgs(DeviceProviders.SelectMany(p => p.Devices).Where(d => d.IsActive).Select(d => d.Native).ToList(), Array.Empty<IDevice>(), Array.Empty<IDevice>()));
        return device;
    }

    public async ValueTask<IDeviceProxy> UpdateAsync(UpdateDeviceOptions options)
    {
        IDeviceProxy device = DeviceProviders.SelectMany(p => p.Devices).Single(d => d.Id.Equals(options.DeviceId, StringComparison.InvariantCultureIgnoreCase));
        foreach (SDK.Common.Models.Port port in options.Ports)
        {
            SDK.Common.Ports.IPort targetPort = device.Native.Ports.First(p => p.Id.Equals(port.Id));
            if (!await _runtimeConverter.TryUpdatePortValueAsync(targetPort, port.Value!))
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed to update port {portId} value to {portValue}", port.Id, port.Value);
                continue;
            }

            _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Updated device [{deviceId}] port [{portName}] value to [{portValue}]", device.Id, port.Name, port.Value);
        }

        DeviceRecord deviceRecord = _storageMapper.Map(device);
        await _deviceRepository.UpdateAsync(deviceRecord);
        DeviceChanged?.Invoke(this, new ObjectChangedEventArgs(device.Native));
        return device;
    }

    private async ValueTask<bool> TryLoadDevice(IDeviceProxy device, DeviceRecord deviceRecord)
    {
        await _runtimeConverter.UpdateValuesAsync(device.Ports, deviceRecord.Ports);
        if (device.IsActive)
        {
            // Connect
            if (!await device.TryConnectAsync())
            {
                return false;
            }

            _logger.LogInformation(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' connected", device.Id);
        }

        return true;
    }
}
