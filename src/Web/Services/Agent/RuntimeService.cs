using System.Runtime.CompilerServices;
using AyBorg.SDK.System.Runtime;
using AyBorg.Web.Services.AppState;

namespace AyBorg.Web.Services.Agent;

public class RuntimeService : IRuntimeService
{
    private readonly ILogger<RuntimeService> _logger;
    private readonly IStateService _stateService;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly Ayborg.Gateway.Agent.V1.AgentRuntime.AgentRuntimeClient _agentRuntimeClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public RuntimeService(ILogger<RuntimeService> logger,
                            IStateService stateService,
                            IAuthorizationHeaderUtilService authorizationHeaderUtilService,
                            Ayborg.Gateway.Agent.V1.AgentRuntime.AgentRuntimeClient agentRuntimeClient)
    {
        _logger = logger;
        _stateService = stateService;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
        _agentRuntimeClient = agentRuntimeClient;
    }

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status.</returns>
    public async ValueTask<EngineMeta> GetStatusAsync()
    {
        Ayborg.Gateway.Agent.V1.GetRuntimeStatusResponse response = await _agentRuntimeClient.GetStatusAsync(new Ayborg.Gateway.Agent.V1.GetRuntimeStatusRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName
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
        Ayborg.Gateway.Agent.V1.StartRunResponse response = await _agentRuntimeClient.StartRunAsync(new Ayborg.Gateway.Agent.V1.StartRunRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName,
            EngineExecutionType = (int)executionType,
            EngineId = string.Empty
        });
        return CreateEngineMeta(response.EngineMetaInfos.First());
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> StopRunAsync()
    {
        Ayborg.Gateway.Agent.V1.StopRunResponse response = await _agentRuntimeClient.StopRunAsync(new Ayborg.Gateway.Agent.V1.StopRunRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName,
            EngineId = string.Empty
        });
        return CreateEngineMeta(response.EngineMetaInfos.First());
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> AbortRunAsync()
    {
        Ayborg.Gateway.Agent.V1.AbortRunResponse response = await _agentRuntimeClient.AbortRunAsync(new Ayborg.Gateway.Agent.V1.AbortRunRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName,
            EngineId = string.Empty
        });
        return CreateEngineMeta(response.EngineMetaInfos.First());
    }

    // /// <summary>
    // /// Creates the hub connection.
    // /// </summary>
    // /// <returns>The hub connection.</returns>
    // public HubConnection CreateHubConnection()
    // {
    //     HubConnection hubConnection = new HubConnectionBuilder()
    //         .WithUrl($"{_stateService.AgentState.UniqueName}/hubs/runtime")
    //         .Build();
    //     return hubConnection;
    // }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EngineMeta CreateEngineMeta(Ayborg.Gateway.Agent.V1.EngineMetaInfo engineMetaInfo)
    {
        return new()
        {
            Id = Guid.Parse(engineMetaInfo.Id),
            State = (EngineState)engineMetaInfo.State,
            ExecutionType = (EngineExecutionType)engineMetaInfo.ExecutionType,
            StartedAt = engineMetaInfo.StartTime.ToDateTime(),
            StoppedAt = engineMetaInfo.StopTime.ToDateTime()
        };
    }
}
