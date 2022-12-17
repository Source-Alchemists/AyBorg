using Ayborg.Gateway.V1;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class RuntimePassthroughServiceV1 : AgentRuntime.AgentRuntimeBase
{
    private readonly ILogger<RuntimePassthroughServiceV1> _logger;
    private readonly IGrpcChannelService _grpcChannelService;

    public RuntimePassthroughServiceV1(ILogger<RuntimePassthroughServiceV1> logger, IGrpcChannelService grpcChannelService)
    {
        _logger = logger;
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetRuntimeStatusResponse> GetStatus(GetRuntimeStatusRequest request, ServerCallContext context)
    {
        AgentRuntime.AgentRuntimeClient client = _grpcChannelService.CreateClient<AgentRuntime.AgentRuntimeClient>(request.AgentUniqueName);
        return await client.GetStatusAsync(request);
    }

    public override async Task<StartRunResponse> StartRun(StartRunRequest request, ServerCallContext context)
    {
        AgentRuntime.AgentRuntimeClient client = _grpcChannelService.CreateClient<AgentRuntime.AgentRuntimeClient>(request.AgentUniqueName);
        return await client.StartRunAsync(request);
    }

    public override async Task<StopRunResponse> StopRun(StopRunRequest request, ServerCallContext context)
    {
        AgentRuntime.AgentRuntimeClient client = _grpcChannelService.CreateClient<AgentRuntime.AgentRuntimeClient>(request.AgentUniqueName);
        return await client.StopRunAsync(request);
    }

    public override async Task<AbortRunResponse> AbortRun(AbortRunRequest request, ServerCallContext context)
    {
        AgentRuntime.AgentRuntimeClient client = _grpcChannelService.CreateClient<AgentRuntime.AgentRuntimeClient>(request.AgentUniqueName);
        return await client.AbortRunAsync(request);
    }
}
