using AyBorg.Web.Shared.Models.Agent;
using static AyBorg.Web.Services.Agent.DeviceManagerService;

namespace AyBorg.Web.Services.Agent;

public interface IDeviceManagerService
{
    ValueTask<IReadOnlyCollection<DeviceProviderMeta>> GetDeviceProvidersAsync(string agentUniqueName);

    ValueTask<DeviceMeta> AddDeviceAsync(AddDeviceRequestOptions options);
    ValueTask<DeviceMeta> RemoveDeviceAsync(CommonDeviceRequestOptions options);
    ValueTask<DeviceMeta> ChangeDeviceStateAsync(ChangeDeviceStateRequestOptions options);
    ValueTask<DeviceMeta> GetDevice(CommonDeviceRequestOptions options);
}
