using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Authorization;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class RuntimePassthroughServiceV1 : Runtime.RuntimeBase
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
        Runtime.RuntimeClient client = _grpcChannelService.CreateClient<Runtime.RuntimeClient>(request.AgentUniqueName);
        return await client.GetStatusAsync(request);
    }

    public override async Task<StartRunResponse> StartRun(StartRunRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        Runtime.RuntimeClient client = _grpcChannelService.CreateClient<Runtime.RuntimeClient>(request.AgentUniqueName);
        return await client.StartRunAsync(request, headers);
    }

    public override async Task<StopRunResponse> StopRun(StopRunRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        Runtime.RuntimeClient client = _grpcChannelService.CreateClient<Runtime.RuntimeClient>(request.AgentUniqueName);
        return await client.StopRunAsync(request, headers);
    }

    public override async Task<AbortRunResponse> AbortRun(AbortRunRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        Runtime.RuntimeClient client = _grpcChannelService.CreateClient<Runtime.RuntimeClient>(request.AgentUniqueName);
        return await client.AbortRunAsync(request, headers);
    }
}
