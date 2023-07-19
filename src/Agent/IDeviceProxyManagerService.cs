using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public interface IDeviceProxyManagerService
{
    IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders { get; }

    void Load();
    ValueTask<IDeviceProxy> AddAsync(AddDeviceOptions options);
    ValueTask<IDeviceProxy> RemoveAsync(string deviceId);
    ValueTask<IDeviceProxy> ChangeStateAsync(ChangeDeviceStateOptions options);
    ValueTask<IDeviceProxy> UpdateAsync(UpdateDeviceOptions options);
}
