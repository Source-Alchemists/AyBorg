using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public sealed class DeviceProxy : IDeviceProxy
{
    private readonly IDevice _device;

    public string Id => _device.Id;

    public string Name => _device.Name;

    public IReadOnlyCollection<string> Categories => _device.Categories;

    public DeviceProxy(ILogger<IDeviceProxy> logger, IDevice device)
    {
        _device = device;
    }
}
