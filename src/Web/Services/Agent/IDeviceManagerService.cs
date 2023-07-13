using AyBorg.Web.Shared.Models.Agent;

namespace AyBorg.Web.Services.Agent;

public interface IDeviceManagerService
{
    ValueTask<IReadOnlyCollection<DeviceProviderMeta>> GetDeviceProvidersAsync(string agentUniqueName);

    ValueTask<DeviceMeta> AddDeviceAsync(string agentUniqueName, string deviceProvideName, string deviceId);
}
