using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public interface IDeviceProxyManagerService
{
    event EventHandler<ObjectChangedEventArgs> DeviceChanged;
    event EventHandler<CollectionChangedEventArgs> DeviceCollectionChanged;
    IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders { get; }

    ValueTask LoadAsync();
    ValueTask<IDeviceProxy> AddAsync(AddDeviceOptions options);
    ValueTask<IDeviceProxy> RemoveAsync(string deviceId);
    ValueTask<IDeviceProxy> ChangeStateAsync(ChangeDeviceStateOptions options);
    ValueTask<IDeviceProxy> UpdateAsync(UpdateDeviceOptions options);
}
