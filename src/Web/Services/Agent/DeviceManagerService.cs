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
        foreach (DeviceProvider? dtoProvider in response.DeviceProviders)
        {
            var devices = new List<DeviceMeta>();
            foreach (Device? dtoDevice in dtoProvider.Devices)
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

    public async ValueTask<DeviceMeta> AddDeviceAsync(string agentUniqueName, string deviceProvideName, string deviceId)
    {
        Device response = await _deviceManagerClient.AddAsync(new AddDeviceRequest
        {
            AgentUniqueName = agentUniqueName,
            DeviceProviderName = deviceProvideName,
            DeviceId = deviceId
        });

        return ToObject(response);
    }

    private static DeviceMeta ToObject(Device dtoDevice) => new()
    {
        Id = dtoDevice.Id,
        Name = dtoDevice.Name,
        Categories = dtoDevice.Categories
    };
}
