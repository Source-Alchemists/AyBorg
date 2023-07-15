using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public sealed class DeviceProviderProxy : IDeviceProviderProxy
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DeviceProviderProxy> _logger;
    private readonly IDeviceProvider _deviceProvider;
    private ImmutableList<IDeviceProxy> _devices = ImmutableList.Create<IDeviceProxy>();
    private bool _isDisposed = false;

    public string Name => _deviceProvider.Name;

    public bool CanAdd => _deviceProvider.CanCreate;

    public IReadOnlyCollection<IDeviceProxy> Devices => _devices;

    public DeviceProviderProxy(ILoggerFactory loggerFactory, ILogger<DeviceProviderProxy> logger, IDeviceProvider deviceManager)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _deviceProvider = deviceManager;
    }

    public async ValueTask<bool> TryInitializeAsync()
    {
        try
        {
            if (_deviceProvider is IInitializable initializable)
            {
                await initializable.OnInitializeAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning((int)EventLogType.Plugin, ex, "Failed to initialize device manager '{name}'", _deviceProvider.Name);
            return false;
        }
    }

    public async ValueTask<IDeviceProxy> AddAsync(AddDeviceOptions options)
    {
        if (!_deviceProvider.CanCreate)
        {
            throw new InvalidOperationException("Device manager does not support adding devices");
        }

        if (Devices.Any(d => d.Name.Equals(options.DeviceId, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"Device with name '{options.DeviceId}' already exists");
        }

        IDevice device = await _deviceProvider.CreateAsync(options.DeviceId);
        var deviceProxy = new DeviceProxy(_loggerFactory.CreateLogger<IDeviceProxy>(), device);
        _devices = _devices.Add(deviceProxy);
        _logger.LogInformation((int)EventLogType.Plugin, "Added device '{id}'", options.DeviceId);
        return deviceProxy;
    }

    public async ValueTask<IDeviceProxy> RemoveAsync(string id)
    {

        IDeviceProxy? deviceProxy = Devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase)) ?? throw new KeyNotFoundException($"Device with id '{id}' does not exist");

        if (deviceProxy.IsConnected)
        {
            if (!await deviceProxy.TryDisconnectAsync())
            {
                _logger.LogWarning((int)EventLogType.Plugin, "Failed to disconnect device '{id}'", id);
                return deviceProxy;
            }
        }

        if (deviceProxy is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _devices = _devices.Remove(deviceProxy);
        _logger.LogInformation((int)EventLogType.Plugin, "Removed device '{id}'", id);
        return deviceProxy;
    }

    public void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            foreach (IDeviceProxy deviceProxy in _devices)
            {
                if (deviceProxy is IDisposable dis)
                {
                    dis.Dispose();
                }
            }

            if (_deviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}