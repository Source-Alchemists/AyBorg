using System.Runtime.CompilerServices;
using AyBorg.SDK.System.Runtime;
using AyBorg.Web.Services.AppState;
using Ayborg.Gateway.Agent.V1;

namespace AyBorg.Web.Services.Agent;

public class RuntimeService : IRuntimeService
{
    private readonly ILogger<RuntimeService> _logger;
    private readonly IStateService _stateService;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly Runtime.RuntimeClient _runtimeClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    /// <param name="runtimeClient">The runtime client.</param>
    public RuntimeService(ILogger<RuntimeService> logger,
                            IStateService stateService,
                            IAuthorizationHeaderUtilService authorizationHeaderUtilService,
                            Runtime.RuntimeClient runtimeClient)
    {
        _logger = logger;
        _stateService = stateService;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
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
        StartRunResponse response = await _runtimeClient.StartRunAsync(new StartRunRequest
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
        StopRunResponse response = await _runtimeClient.StopRunAsync(new StopRunRequest
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
        AbortRunResponse response = await _runtimeClient.AbortRunAsync(new AbortRunRequest
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
    private static EngineMeta CreateEngineMeta(EngineMetaDto engineMetaInfo) => new()
    {
        Id = Guid.Parse(engineMetaInfo.Id),
        State = (EngineState)engineMetaInfo.State,
        ExecutionType = (EngineExecutionType)engineMetaInfo.ExecutionType,
        StartedAt = engineMetaInfo.StartTime.ToDateTime(),
        StoppedAt = engineMetaInfo.StopTime.ToDateTime()
    };
}
