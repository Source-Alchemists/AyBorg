using System.Runtime.CompilerServices;
using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common;
using AyBorg.SDK.System.Runtime;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public class RuntimeService : IRuntimeService
{
    private readonly ILogger<RuntimeService> _logger;
    private readonly Runtime.RuntimeClient _runtimeClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="runtimeClient">The runtime client.</param>
    public RuntimeService(ILogger<RuntimeService> logger,
                            Runtime.RuntimeClient runtimeClient)
    {
        _logger = logger;
        _runtimeClient = runtimeClient;
    }

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status.</returns>
    public async ValueTask<EngineMeta> GetStatusAsync(string serviceUniqueName)
    {
        try
        {
            GetRuntimeStatusResponse response = await _runtimeClient.GetStatusAsync(new GetRuntimeStatusRequest
            {
                AgentUniqueName = serviceUniqueName
            });

            return CreateEngineMeta(response.EngineMetaInfos.First());
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to get status!");
            return null!;
        }
    }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> StartRunAsync(string serviceUniqueName, EngineExecutionType executionType)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Start run [{executionType}].", executionType);
            StartRunResponse response = await _runtimeClient.StartRunAsync(new StartRunRequest
            {
                AgentUniqueName = serviceUniqueName,
                EngineExecutionType = (int)executionType,
                EngineId = string.Empty
            });

            return CreateEngineMeta(response.EngineMetaInfos.First());
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to start run");
            return null!;
        }
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> StopRunAsync(string serviceUniqueName)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Stop run.");
            StopRunResponse response = await _runtimeClient.StopRunAsync(new StopRunRequest
            {
                AgentUniqueName = serviceUniqueName,
                EngineId = string.Empty
            });

            return CreateEngineMeta(response.EngineMetaInfos.First());
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to stop run");
            return null!;
        }
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> AbortRunAsync(string serviceUniqueName)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Abort run.");
            AbortRunResponse response = await _runtimeClient.AbortRunAsync(new AbortRunRequest
            {
                AgentUniqueName = serviceUniqueName,
                EngineId = string.Empty
            });

            return CreateEngineMeta(response.EngineMetaInfos.First());
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to abort run");
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
