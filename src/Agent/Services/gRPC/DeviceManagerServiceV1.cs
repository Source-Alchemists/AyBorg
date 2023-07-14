using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Projects;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class DeviceManagerServiceV1 : Ayborg.Gateway.Agent.V1.DeviceManager.DeviceManagerBase
{
    private readonly ILogger<DeviceManagerServiceV1> _logger;
    private readonly IDeviceProxyManagerService _deviceManagerService;

    public DeviceManagerServiceV1(ILogger<DeviceManagerServiceV1> logger, IDeviceProxyManagerService deviceManagerService)
    {
        _logger = logger;
        _deviceManagerService = deviceManagerService;
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

    private static DeviceDto ToDto(IDeviceProxy device)
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

        return deviceDto;
    }
}
