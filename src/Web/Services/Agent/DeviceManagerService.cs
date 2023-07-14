using Ayborg.Gateway.Agent.V1;
using AyBorg.Web.Shared.Models.Agent;

namespace AyBorg.Web.Services.Agent;

public class DeviceManagerService : IDeviceManagerService
{
    private readonly ILogger<DeviceManagerService> _logger;
    private readonly DeviceManager.DeviceManagerClient _deviceManagerClient;

    public DeviceManagerService(ILogger<DeviceManagerService> logger, DeviceManager.DeviceManagerClient deviceManagerClient)
    {
        _logger = logger;
        _deviceManagerClient = deviceManagerClient;
    }

    public async ValueTask<IReadOnlyCollection<DeviceProviderMeta>> GetDeviceProvidersAsync(string agentUniqueName)
    {
        DeviceProviderCollectionResponse response = await _deviceManagerClient.GetAvailableProvidersAsync(new DefaultAgentRequest
        {
            AgentUniqueName = agentUniqueName
        });

        var result = new List<DeviceProviderMeta>();
        foreach (DeviceProviderDto? dtoProvider in response.DeviceProviders)
        {
            var devices = new List<DeviceMeta>();
            foreach (DeviceDto? dtoDevice in dtoProvider.Devices)
            {
                DeviceMeta device = ToObject(dtoDevice);
                devices.Add(device);
            }

            result.Add(new DeviceProviderMeta
            {
                Name = dtoProvider.Name,
                CanAdd = dtoProvider.CanAdd,
                Devices = devices
            });
        }

        return result;
    }

    public async ValueTask<DeviceMeta> AddDeviceAsync(AddDeviceOptions options)
    {
        DeviceDto response = await _deviceManagerClient.AddAsync(new AddDeviceRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceProviderName = options.DeviceProviderName,
            DeviceId = options.DeviceId
        });

        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> RemoveDeviceAsync(RemoveDeviceOptions options)
    {
        DeviceDto response = await _deviceManagerClient.RemoveAsync(new RemoveDeviceRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId
        });

        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> ChangeDeviceStateAsync(ChangeDeviceStateOptions options)
    {
        DeviceDto response = await _deviceManagerClient.ChangeStateAsync(new DeviceStateRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId,
            Activate = options.Activate
        });

        return ToObject(response);
    }

    private static DeviceMeta ToObject(DeviceDto dtoDevice) => new()
    {
        Id = dtoDevice.Id,
        Name = dtoDevice.Name,
        IsActive = dtoDevice.IsActive,
        IsConnected = dtoDevice.IsConnected,
        Categories = dtoDevice.Categories
    };

    public sealed record AddDeviceOptions (string AgentUniqueName, string DeviceProviderName, string DeviceId);
    public sealed record RemoveDeviceOptions (string AgentUniqueName, string DeviceId);
    public sealed record ChangeDeviceStateOptions (string AgentUniqueName, string DeviceId, bool Activate);
}
