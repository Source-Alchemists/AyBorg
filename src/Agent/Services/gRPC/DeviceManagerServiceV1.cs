using Ayborg.Gateway.Agent.V1;
using AyBorg.Data.Mapper;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Projects;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class DeviceManagerServiceV1 : Ayborg.Gateway.Agent.V1.DeviceManager.DeviceManagerBase
{
    private readonly ILogger<DeviceManagerServiceV1> _logger;
    private readonly IDeviceProxyManagerService _deviceManagerService;
    private readonly IRuntimeMapper _runtimeMapper;
    private readonly IRpcMapper _rpcMapper;

    public DeviceManagerServiceV1(ILogger<DeviceManagerServiceV1> logger, IDeviceProxyManagerService deviceManagerService, IRuntimeMapper runtimeMapper, IRpcMapper rpcMapper)
    {
        _logger = logger;
        _deviceManagerService = deviceManagerService;
        _runtimeMapper = runtimeMapper;
        _rpcMapper = rpcMapper;
    }

    public override Task<DeviceProviderCollectionResponse> GetAvailableProviders(DefaultAgentRequest request, ServerCallContext context)
    {
        var response = new DeviceProviderCollectionResponse();

        foreach (IDeviceProviderProxy provider in _deviceManagerService.DeviceProviders)
        {
            var provideDto = new DeviceProviderDto
            {
                Name = provider.Name,
                CanAdd = provider.CanAdd
            };

            foreach (IDeviceProxy device in provider.Devices)
            {
                DeviceDto deviceDto = ToDto(device);

                provideDto.Devices.Add(deviceDto);
            }

            response.DeviceProviders.Add(provideDto);
        }

        return Task.FromResult(response);
    }

    public override async Task<DeviceDto> Add(AddDeviceRequest request, ServerCallContext context)
    {
        IDeviceProxy newDevice = await _deviceManagerService.AddAsync(new AddDeviceOptions(request.DeviceProviderName, request.DeviceId));
        return ToDto(newDevice);
    }

    public override async Task<DeviceDto> Remove(RemoveDeviceRequest request, ServerCallContext context)
    {
        IDeviceProxy device = await _deviceManagerService.RemoveAsync(request.DeviceId);
        return ToDto(device);
    }

    public override async Task<DeviceDto> ChangeState(DeviceStateRequest request, ServerCallContext context)
    {
        IDeviceProxy device = await _deviceManagerService.ChangeStateAsync(new ChangeDeviceStateOptions(request.DeviceId, request.Activate));
        return ToDto(device);
    }

    public override Task<DeviceDto> GetDevice(GetDeviceRequest request, ServerCallContext context)
    {
        IDeviceProxy device = _deviceManagerService.DeviceProviders.SelectMany(p => p.Devices).Single(d => d.Id.Equals(request.DeviceId, StringComparison.InvariantCultureIgnoreCase));
        return Task.FromResult(ToDto(device, false));
    }

    private DeviceDto ToDto(IDeviceProxy device, bool skipPorts = true)
    {
        var deviceDto = new DeviceDto
        {
            Id = device.Id,
            Name = device.Name,
            IsActive = device.IsActive,
            IsConnected = device.IsConnected
        };
        foreach (string category in device.Categories)
        {
            deviceDto.Categories.Add(category);
        }

        if (!skipPorts)
        {
            foreach (SDK.Common.Ports.IPort port in device.Native.Ports)
            {
                deviceDto.Ports.Add(_rpcMapper.ToRpc(_runtimeMapper.FromRuntime(port)));
            }
        }

        return deviceDto;
    }
}
