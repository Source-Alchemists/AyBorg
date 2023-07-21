using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public interface IDeviceProxyManagerService
{
    IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders { get; }

    ValueTask LoadAsync();
    ValueTask<IDeviceProxy> AddAsync(AddDeviceOptions options);
    ValueTask<IDeviceProxy> RemoveAsync(string deviceId);
    ValueTask<IDeviceProxy> ChangeStateAsync(ChangeDeviceStateOptions options);
    ValueTask<IDeviceProxy> UpdateAsync(UpdateDeviceOptions options);
}
