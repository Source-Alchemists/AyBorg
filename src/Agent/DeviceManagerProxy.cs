using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public sealed class DeviceManagerProxy : IDeviceManagerProxy
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DeviceManagerProxy> _logger;
    private readonly IDeviceManager _deviceManager;
    private bool _isDisposed = false;

    public bool CanAdd => _deviceManager.CanAdd;

    public bool CanRemove => _deviceManager.CanRemove;

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
        if (_deviceManager.CanAdd)
        {
            IDevice device = await _deviceManager.AddAsync(id);
            return new DeviceProxy(_loggerFactory.CreateLogger<IDeviceProxy>(), device);
        }

        throw new InvalidOperationException("Device manager does not support adding devices");
    }

    public async ValueTask RemoveAsync(string id)
    {
        if (_deviceManager.CanRemove)
        {
            await _deviceManager.RemoveAsync(id);
            return;
        }

        throw new InvalidOperationException("Device manager does not support removing devices");
    }

    public void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
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
