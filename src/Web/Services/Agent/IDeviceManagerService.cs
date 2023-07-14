using AyBorg.Web.Shared.Models.Agent;
using static AyBorg.Web.Services.Agent.DeviceManagerService;

namespace AyBorg.Web.Services.Agent;

public interface IDeviceManagerService
{
    ValueTask<IReadOnlyCollection<DeviceProviderMeta>> GetDeviceProvidersAsync(string agentUniqueName);

    ValueTask<DeviceMeta> AddDeviceAsync(AddDeviceOptions options);
    ValueTask<DeviceMeta> RemoveDeviceAsync(RemoveDeviceOptions options);
    ValueTask<DeviceMeta> ChangeDeviceStateAsync(ChangeDeviceStateOptions options);
}
