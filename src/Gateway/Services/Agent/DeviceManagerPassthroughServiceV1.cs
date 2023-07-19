using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Authorization;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class DeviceManagerPassthroughServiceV1 : DeviceManager.DeviceManagerBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public DeviceManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<DeviceProviderCollectionResponse> GetAvailableProviders(DefaultAgentRequest request, ServerCallContext context)
    {
        DeviceManager.DeviceManagerClient client = _grpcChannelService.CreateClient<DeviceManager.DeviceManagerClient>(request.AgentUniqueName);
        return await client.GetAvailableProvidersAsync(request);
    }

    public override async Task<DeviceDto> Add(AddDeviceRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        DeviceManager.DeviceManagerClient client = _grpcChannelService.CreateClient<DeviceManager.DeviceManagerClient>(request.AgentUniqueName);
        return await client.AddAsync(request, headers);
    }

    public override async Task<DeviceDto> Remove(RemoveDeviceRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        DeviceManager.DeviceManagerClient client = _grpcChannelService.CreateClient<DeviceManager.DeviceManagerClient>(request.AgentUniqueName);
        return await client.RemoveAsync(request, headers);
    }

    public override async Task<DeviceDto> ChangeState(DeviceStateRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        DeviceManager.DeviceManagerClient client = _grpcChannelService.CreateClient<DeviceManager.DeviceManagerClient>(request.AgentUniqueName);
        return await client.ChangeStateAsync(request, headers);
    }

    public override async Task<DeviceDto> GetDevice(GetDeviceRequest request, ServerCallContext context)
    {
        DeviceManager.DeviceManagerClient client = _grpcChannelService.CreateClient<DeviceManager.DeviceManagerClient>(request.AgentUniqueName);
        return await client.GetDeviceAsync(request);
    }

    public override async Task<DeviceDto> UpdateDevice(UpdateDeviceRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        DeviceManager.DeviceManagerClient client = _grpcChannelService.CreateClient<DeviceManager.DeviceManagerClient>(request.AgentUniqueName);
        return await client.UpdateDeviceAsync(request, headers);
    }
}
