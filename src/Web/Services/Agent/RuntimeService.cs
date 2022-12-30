using System.Runtime.CompilerServices;
using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.System.Runtime;
using AyBorg.Web.Services.AppState;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public class RuntimeService : IRuntimeService
{
    private readonly ILogger<RuntimeService> _logger;
    private readonly IStateService _stateService;
    private readonly Runtime.RuntimeClient _runtimeClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="runtimeClient">The runtime client.</param>
    public RuntimeService(ILogger<RuntimeService> logger,
                            IStateService stateService,
                            Runtime.RuntimeClient runtimeClient)
    {
        _logger = logger;
        _stateService = stateService;
        _runtimeClient = runtimeClient;
    }

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <returns>The status.</returns>
    public async ValueTask<EngineMeta> GetStatusAsync()
    {
        return await GetStatusAsync(_stateService.AgentState.UniqueName);
    }

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status.</returns>
    public async ValueTask<EngineMeta> GetStatusAsync(string serviceUniqueName)
    {
        GetRuntimeStatusResponse response = await _runtimeClient.GetStatusAsync(new GetRuntimeStatusRequest
        {
            AgentUniqueName = serviceUniqueName
        });
        return CreateEngineMeta(response.EngineMetaInfos.First());
    }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> StartRunAsync(EngineExecutionType executionType)
    {
        try
        {
            StartRunResponse response = await _runtimeClient.StartRunAsync(new StartRunRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                EngineExecutionType = (int)executionType,
                EngineId = string.Empty
            });
            return CreateEngineMeta(response.EngineMetaInfos.First());
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to start run");
            return null!;
        }
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> StopRunAsync()
    {
        try
        {
            StopRunResponse response = await _runtimeClient.StopRunAsync(new StopRunRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                EngineId = string.Empty
            });
            return CreateEngineMeta(response.EngineMetaInfos.First());
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to stop run");
            return null!;
        }
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> AbortRunAsync()
    {
        try
        {
            AbortRunResponse response = await _runtimeClient.AbortRunAsync(new AbortRunRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                EngineId = string.Empty
            });
            return CreateEngineMeta(response.EngineMetaInfos.First());
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to abort run");
            return null!;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EngineMeta CreateEngineMeta(EngineMetaDto engineMetaInfo) => new()
    {
        Id = Guid.Parse(engineMetaInfo.Id),
        State = (EngineState)engineMetaInfo.State,
        ExecutionType = (EngineExecutionType)engineMetaInfo.ExecutionType,
        StartedAt = engineMetaInfo.StartTime.ToDateTime(),
        StoppedAt = engineMetaInfo.StopTime.ToDateTime()
    };
}
