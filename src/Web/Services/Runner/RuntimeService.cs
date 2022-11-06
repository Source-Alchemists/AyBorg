using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using Autodroid.SDK.System.Runtime;

namespace Autodroid.Web.Services.Agent;

public class RuntimeService : IRuntimeService
{
    private readonly ILogger<RuntimeService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IStateService _stateService;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public RuntimeService(ILogger<RuntimeService> logger, 
                            HttpClient httpClient, 
                            IStateService stateService, 
                            IAuthorizationHeaderUtilService authorizationHeaderUtilService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _stateService = stateService;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
    }

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <returns>The status.</returns>
    public async Task<EngineMeta> GetStatusAsync()
    {
        return await GetStatusAsync(_stateService.AgentState.BaseUrl);
    }

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The status.</returns>
    public async Task<EngineMeta> GetStatusAsync(string baseUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/runtime/status");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null!;
        }
        var status = await response.Content.ReadFromJsonAsync<EngineMeta>();
        return status!;
    }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns>The status</returns>
    public async Task<EngineMeta> StartRunAsync(EngineExecutionType executionType)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_stateService.AgentState.BaseUrl}/runtime/start/{executionType}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogWarning("No status received from Agent. Indicating that the Agent is not started.");
            return null!;
        }
        var status = await response.Content.ReadFromJsonAsync<EngineMeta>();
        return status!;
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async Task<EngineMeta> StopRunAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_stateService.AgentState.BaseUrl}/runtime/stop");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogWarning("No status received from Agent. Indicating that the Agent is not started.");
            return null!;
        }

        var status = await response.Content.ReadFromJsonAsync<EngineMeta>();
        return status!;
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async Task<EngineMeta> AbortRunAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_stateService.AgentState.BaseUrl}/runtime/abort");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogWarning("No status received from Agent. Indicating that the Agent is not in a abortable state.");
            return null!;
        }

        var status = await response.Content.ReadFromJsonAsync<EngineMeta>();
        return status!;
    }

    /// <summary>
    /// Creates the hub connection.
    /// </summary>
    /// <returns>The hub connection.</returns>
    public HubConnection CreateHubConnection()
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_stateService.AgentState.BaseUrl}/hubs/runtime")
            .Build();
        return hubConnection;
    }
}