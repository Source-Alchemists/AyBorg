using AyBorg.SDK.Common;

namespace AyBorg.Agent.Services;

internal sealed class DeviceManager : IDeviceManager
{
    private readonly IDeviceProxyManagerService _deviceProxyManagerService;

    public event EventHandler<ObjectChangedEventArgs>? DeviceChanged;
    public event EventHandler<CollectionChangedEventArgs>? DeviceCollectionChanged;

    public IEnumerable<IDevice> Devices => _deviceProxyManagerService.DeviceProviders.SelectMany(p => p.Devices).Where(d => d.IsActive).Select(d => d.Native).ToList();

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

    public T GetDevice<T>(string deviceId) where T : IDevice => (T)Devices.FirstOrDefault(d => d.Id.Equals(deviceId, StringComparison.InvariantCultureIgnoreCase))!;
    public IEnumerable<T> GetDevices<T>() where T : IDevice => Devices.Where(d => d is T).Cast<T>();
}
