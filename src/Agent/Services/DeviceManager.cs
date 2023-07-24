using AyBorg.SDK.Common;

namespace AyBorg.Agent.Services;

internal sealed class DeviceManager : IDeviceManager
{
    private readonly IDeviceProxyManagerService _deviceProxyManagerService;

    /// <inheritdoc/>
    public event EventHandler<ObjectChangedEventArgs>? DeviceChanged;

    /// <inheritdoc/>
    public event EventHandler<CollectionChangedEventArgs>? DeviceCollectionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceManager"/> class.
    /// </summary>
    /// <param name="deviceProxyManagerService">The device proxy manager service.</param>
    public DeviceManager(IDeviceProxyManagerService deviceProxyManagerService)
    {
        _deviceProxyManagerService = deviceProxyManagerService;
        _deviceProxyManagerService.DeviceCollectionChanged += (s, e) =>
        {
            DeviceCollectionChanged?.Invoke(this, e);
        };

        _deviceProxyManagerService.DeviceChanged += (s, e) =>
        {
            DeviceChanged?.Invoke(this, e);
        };
    }

    /// <inheritdoc/>
    public T GetDevice<T>(string deviceId) where T : IDevice => (T)GetDevices<T>().FirstOrDefault(d => d.Id.Equals(deviceId, StringComparison.InvariantCultureIgnoreCase))!;

    /// <inheritdoc/>
    public IEnumerable<T> GetDevices<T>() where T : IDevice => GetDevices().Where(d => d is T).Cast<T>();

    private IEnumerable<IDevice> GetDevices() => _deviceProxyManagerService.DeviceProviders.SelectMany(p => p.Devices).Where(d => d.IsActive).Select(d => d.Native).ToList();
}
