using AyBorg.SDK.Projects;

namespace AyBorg.Data.Agent;

public interface IDeviceToStorageMapper
{
    /// <summary>
    /// Maps the specified device proxy.
    /// </summary>
    /// <param name="deviceProxy">The device proxy.</param>
    /// <returns></returns>
    DeviceRecord Map(IDeviceProxy deviceProxy);
}
