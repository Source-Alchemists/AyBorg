using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public class DeviceProxy : IDeviceProxy
{
    public string Id => Native.Id;
    public string Name => Native.Name;
    public bool IsActive { get; private set; }
    public bool IsConnected => Native.IsConnected;
    public IReadOnlyCollection<string> Categories => Native.Categories;
    public IDevice Native { get; }

    public DeviceProxy(ILogger<IDeviceProxy> logger, IDevice device)
    {
        Native = device;
    }

    public async ValueTask<bool> TryConnectAsync()
    {
        IsActive = await Native.TryConnectAsync();
        return IsActive;
    }

    public async ValueTask<bool> TryDisconnectAsync()
    {
        IsActive = !await Native.TryDisconnectAsync();
        return !IsActive;
    }
}
