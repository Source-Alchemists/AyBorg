using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public interface IDeviceManagerService
{
    IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders { get; }

    ValueTask<IDeviceProxy> AddAsync(string providerName, string deviceId);
    ValueTask<IDeviceProxy> RemoveAsync(string deviceId);
}
