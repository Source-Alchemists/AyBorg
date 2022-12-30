using System.Runtime.CompilerServices;
using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System.Runtime;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class RuntimeServiceV1 : Ayborg.Gateway.Agent.V1.Runtime.RuntimeBase
{
    private readonly ILogger<RuntimeServiceV1> _logger;
    private readonly IEngineHost _engineHost;

    public RuntimeServiceV1(ILogger<RuntimeServiceV1> logger, IEngineHost engineHost)
    {
        _logger = logger;
        _engineHost = engineHost;
    }

    public override async Task<GetRuntimeStatusResponse> GetStatus(GetRuntimeStatusRequest request, ServerCallContext context)
    {
        EngineMeta status = await _engineHost.GetEngineStatusAsync();
        ThrowIfNull(status);

        var result = new GetRuntimeStatusResponse();
        result.EngineMetaInfos.Add(CreateEngineMetaInfo(status));
        return result;
    }

    public override async Task<StartRunResponse> StartRun(StartRunRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        EngineMeta status = await _engineHost.StartRunAsync((EngineExecutionType)request.EngineExecutionType);
        ThrowIfNull(status);

        var result = new StartRunResponse();
        result.EngineMetaInfos.Add(CreateEngineMetaInfo(status));
        return result;
    }

    public override async Task<StopRunResponse> StopRun(StopRunRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        EngineMeta status = await _engineHost.StopRunAsync();
        ThrowIfNull(status);

        var result = new StopRunResponse();
        result.EngineMetaInfos.Add(CreateEngineMetaInfo(status));
        return result;
    }

    public override async Task<AbortRunResponse> AbortRun(AbortRunRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        EngineMeta status = await _engineHost.AbortRunAsync();
        ThrowIfNull(status);

        var result = new AbortRunResponse();
        result.EngineMetaInfos.Add(CreateEngineMetaInfo(status));
        return result;
    }

    private void ThrowIfNull(EngineMeta status)
    {
        if (status == null)
        {
            _logger.LogWarning("No engine status found.");
            throw new RpcException(new Status(StatusCode.NotFound, "No engine status found."));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EngineMetaDto CreateEngineMetaInfo(EngineMeta status)
    {
        return new EngineMetaDto
        {
            Id = status.Id.ToString(),
            State = (int)status.State,
            ExecutionType = (int)status.ExecutionType,
            StartTime = Timestamp.FromDateTime(status.StartedAt.ToUniversalTime()),
            StopTime = Timestamp.FromDateTime(status.StoppedAt.ToUniversalTime())
        };
    }
}
