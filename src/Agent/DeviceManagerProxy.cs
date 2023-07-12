using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public sealed class DeviceManagerProxy : IDeviceManagerProxy
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DeviceManagerProxy> _logger;
    private readonly IDeviceManager _deviceManager;
    private ImmutableList<IDeviceProxy> _devices = ImmutableList.Create<IDeviceProxy>();
    private bool _isDisposed = false;

    public bool CanAdd => _deviceManager.CanCreate;

    public IReadOnlyCollection<IDeviceProxy> Devices => _devices;

    public DeviceManagerProxy(ILoggerFactory loggerFactory, ILogger<DeviceManagerProxy> logger, IDeviceManager deviceManager)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _deviceManager = deviceManager;
    }

    public async ValueTask<bool> TryInitializeAsync()
    {
        try
        {
            if (_deviceManager is IInitializable initializable)
            {
                await initializable.OnInitializeAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning((int)EventLogType.Plugin, ex, "Failed to initialize device manager '{name}'", _deviceManager.Name);
            return false;
        }
    }

    public async ValueTask<IDeviceProxy> AddAsync(string id)
    {
        if (!_deviceManager.CanCreate)
        {
            throw new InvalidOperationException("Device manager does not support adding devices");
        }

        if (Devices.Any(d => d.Name.Equals(id, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"Device with name '{id}' already exists");
        }

        IDevice device = await _deviceManager.CreateAsync(id);
        var deviceProxy = new DeviceProxy(_loggerFactory.CreateLogger<IDeviceProxy>(), device);
        _devices = _devices.Add(deviceProxy);
        _logger.LogInformation((int)EventLogType.Plugin, "Added device '{id}'", id);
        return deviceProxy;
    }

    public ValueTask<IDeviceProxy> RemoveAsync(string id)
    {

        IDeviceProxy? deviceProxy = Devices.FirstOrDefault(d => d.Name.Equals(id, StringComparison.InvariantCultureIgnoreCase)) ?? throw new KeyNotFoundException($"Device with name '{id}' does not exist");

        if (deviceProxy is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _devices = _devices.Remove(deviceProxy);
        _logger.LogInformation((int)EventLogType.Plugin, "Removed device '{id}'", id);
        return ValueTask.FromResult(deviceProxy);
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

            if (_deviceManager is IDisposable disposable)
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
