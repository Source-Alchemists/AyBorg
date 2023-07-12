using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public sealed class DeviceProxy : IDeviceProxy {

    private readonly IDevice _device;

    public DeviceProxy(ILogger<IDeviceProxy> logger, IDevice device)
    {
        _device = device;
    }
}
