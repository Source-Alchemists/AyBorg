using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using AyBorg.SDK.Data.DTOs;
using Newtonsoft.Json;
using System.Text;

namespace AyBorg.Web.Services.Agent;

public class FlowService : IFlowService
{
    private readonly ILogger<FlowService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAgentCacheService _AgentCacheService;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="agentCacheService">The agent cache service.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public FlowService(ILogger<FlowService> logger, 
                        HttpClient httpClient, 
                        IAgentCacheService AgentCacheService, 
                        IAuthorizationHeaderUtilService authorizationHeaderUtilService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _AgentCacheService = AgentCacheService;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
    }

    /// <summary>
    /// Creates the hub connection.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The hub connection.</returns>
    public HubConnection CreateHubConnection(string baseUrl)
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/flow")
            .Build();
        return hubConnection;
    }

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The steps.</returns>
    public async Task<IEnumerable<StepDto>> GetStepsAsync(string baseUrl)
    {   
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<StepDto>>($"{baseUrl}/flow/steps");
        return response!;
    }

    /// <summary>
    /// Gets the links.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The links.</returns>
    public async Task<IEnumerable<LinkDto>> GetLinksAsync(string baseUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/flow/links");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var links = JsonConvert.DeserializeObject<IEnumerable<LinkDto>>(content);
        return links!;
    }

    /// <summary>
    /// Adds the step asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async Task<StepDto> AddStepAsync(string baseUrl, Guid stepId, int x, int y)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/flow/steps/{stepId}/{x}/{y}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        var step = await response.Content.ReadFromJsonAsync<StepDto>();
        if (step == null)
        {
            _logger.LogWarning("No steps received from Agent.");
            return null!;
        }
        return step;
    }

    /// <summary>
    /// Removes the step asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <returns></returns>
    public async Task<bool> TryRemoveStepAsync(string baseUrl, Guid stepId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/flow/steps/{stepId}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Moves the step asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async Task<bool> TryMoveStepAsync(string baseUrl, Guid stepId, int x, int y)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/flow/steps/{stepId}/{x}/{y}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Add link between ports asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    public async Task<bool> TryAddLinkAsync(string baseUrl, Guid sourcePortId, Guid targetPortId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/flow/links/{sourcePortId}/{targetPortId}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Removes the link asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    public async Task<bool> TryRemoveLinkAsync(string baseUrl, Guid linkId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/flow/links/{linkId}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Gets the port asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="portId">The port identifier.</param>
    /// <returns></returns>
    public async Task<PortDto> GetPortAsync(string baseUrl, Guid portId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/flow/ports/{portId}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        if(response.StatusCode == HttpStatusCode.OK) {
            var port = await response.Content.ReadFromJsonAsync<PortDto>();
            return port!;
        }
        return null!;
    }

    /// <summary>
    /// Gets the port for the given iteration.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    public async Task<PortDto> GetPortAsync(string baseUrl, Guid portId, Guid iterationId)
    {
        return await _AgentCacheService.GetOrCreatePortEntryAsync(baseUrl, portId, iterationId);
    }

    /// <summary>
    /// Try to set the port value asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    public async Task<bool> TrySetPortValueAsync(string baseUrl, PortDto port)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/flow/ports")
        {
            Content = new StringContent(JsonConvert.SerializeObject(port), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Gets the step execution time asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    public async Task<long> GetStepExecutionTimeAsync(string baseUrl, Guid stepId, Guid iterationId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/flow/steps/{stepId}/{iterationId}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        if(response.StatusCode == HttpStatusCode.OK) {
            var executionTime = await response.Content.ReadFromJsonAsync<long>();
            return executionTime;
        }
        return 0;
    }
}