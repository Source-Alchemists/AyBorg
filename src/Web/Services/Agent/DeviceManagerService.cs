using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.Web.Shared.Models.Agent;

namespace AyBorg.Web.Services.Agent;

public class DeviceManagerService : IDeviceManagerService
{
    private readonly DeviceManager.DeviceManagerClient _deviceManagerClient;
    private readonly IRpcMapper _rpcMapper;

    public DeviceManagerService(DeviceManager.DeviceManagerClient deviceManagerClient, IRpcMapper rpcMapper)
    {
        _deviceManagerClient = deviceManagerClient;
        _rpcMapper = rpcMapper;
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
                Prefix = dtoProvider.Prefix,
                CanAdd = dtoProvider.CanAdd,
                Devices = devices
            });
        }

        return result;
    }

    public async ValueTask<DeviceMeta> AddDeviceAsync(AddDeviceRequestOptions options)
    {
        DeviceDto response = await _deviceManagerClient.AddAsync(new AddDeviceRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceProviderName = options.DeviceProviderName,
            DeviceId = options.DeviceId,
            DevicePrefix = options.DevicePrefix
        });

        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> RemoveDeviceAsync(CommonDeviceRequestOptions options)
    {
        DeviceDto response = await _deviceManagerClient.RemoveAsync(new RemoveDeviceRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId
        });

        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> ChangeDeviceStateAsync(ChangeDeviceStateRequestOptions options)
    {
        DeviceDto response = await _deviceManagerClient.ChangeStateAsync(new DeviceStateRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId,
            Activate = options.Activate
        });

        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> GetDeviceAsync(CommonDeviceRequestOptions options)
    {
        DeviceDto response = await _deviceManagerClient.GetDeviceAsync(new GetDeviceRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId
        });

        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> UpdateDeviceAsync(UpdateDeviceRequestOptions options)
    {
        var request = new UpdateDeviceRequest{
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId
        };

        foreach(Port port in options.Ports)
        {
            request.Ports.Add(_rpcMapper.ToRpc(port));
        }

        return ToObject(await _deviceManagerClient.UpdateDeviceAsync(request));
    }

    private DeviceMeta ToObject(DeviceDto dtoDevice)
    {
        var ports = new List<Port>();
        foreach (PortDto? portDto in dtoDevice.Ports)
        {
            ports.Add(_rpcMapper.FromRpc(portDto));
        }

        return new DeviceMeta()
        {
            Id = dtoDevice.Id,
            Name = dtoDevice.Name,
            Manufacturer = dtoDevice.Manufacturer,
            IsActive = dtoDevice.IsActive,
            IsConnected = dtoDevice.IsConnected,
            Categories = dtoDevice.Categories,
            Ports = ports
        };
    }

    public sealed record CommonDeviceRequestOptions(string AgentUniqueName, string DeviceId);
    public sealed record AddDeviceRequestOptions(string AgentUniqueName, string DeviceProviderName, string DevicePrefix, string DeviceId);
    public sealed record ChangeDeviceStateRequestOptions(string AgentUniqueName, string DeviceId, bool Activate);
    public sealed record UpdateDeviceRequestOptions(string AgentUniqueName, string DeviceId, IReadOnlyCollection<Port> Ports);
}
