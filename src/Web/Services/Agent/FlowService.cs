using System.Text;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Data.Bindings;
using Grpc.Core;
using Newtonsoft.Json;

namespace AyBorg.Web.Services.Agent;

public class FlowService : IFlowService
{
    private readonly ILogger<FlowService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly Ayborg.Gateway.V1.AgentEditor.AgentEditorClient _agentEditorClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="agentCacheService">The agent cache service.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public FlowService(ILogger<FlowService> logger,
                        IAuthorizationHeaderUtilService authorizationHeaderUtilService,
                        Ayborg.Gateway.V1.AgentEditor.AgentEditorClient agentEditorClient,
                        HttpClient httpClient)
    {
        _logger = logger;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
        _agentEditorClient = agentEditorClient;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The steps.</returns>
    public async ValueTask<IEnumerable<Step>> GetStepsAsync(string serviceUniqueName)
    {
        Ayborg.Gateway.V1.GetFlowStepsResponse response = await _agentEditorClient.GetFlowStepsAsync(new Ayborg.Gateway.V1.GetFlowStepsRequest { AgentUniqueName = serviceUniqueName });
        var result = new List<Step>();
        foreach (Ayborg.Gateway.V1.Step? s in response.Steps)
        {
            result.Add(RpcMapper.FromRpc(s));
        }

        return result;
    }

    /// <summary>
    /// Gets the links.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The links.</returns>
    public async ValueTask<IEnumerable<Link>> GetLinksAsync(string serviceUniqueName)
    {
        Ayborg.Gateway.V1.GetFlowLinksResponse response = await _agentEditorClient.GetFlowLinksAsync(new Ayborg.Gateway.V1.GetFlowLinksRequest { AgentUniqueName = serviceUniqueName });
        var result = new List<Link>();
        foreach (Ayborg.Gateway.V1.Link? l in response.Links)
        {
            result.Add(RpcMapper.FromRpc(l));
        }

        return result;
    }

    /// <summary>
    /// Adds the step asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async ValueTask<Step> AddStepAsync(string serviceUniqueName, Guid stepId, int x, int y)
    {
        Ayborg.Gateway.V1.AddFlowStepResponse response = await _agentEditorClient.AddFlowStepAsync(new Ayborg.Gateway.V1.AddFlowStepRequest
        {
            AgentUniqueName = serviceUniqueName,
            StepId = stepId.ToString(),
            X = x,
            Y = y
        });

        return RpcMapper.FromRpc(response.Step);
    }

    /// <summary>
    /// Removes the step asynchronous.
    /// </summary>
    /// <param name="baseUrl">The service unique name.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRemoveStepAsync(string serviceUniqueName, Guid stepId)
    {
        try
        {
            _ = await _agentEditorClient.DeleteFlowStepAsync(new Ayborg.Gateway.V1.DeleteFlowStepRequest
            {
                AgentUniqueName = serviceUniqueName,
                StepId = stepId.ToString()
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error deleting step");
            return false;
        }
    }

    /// <summary>
    /// Moves the step asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryMoveStepAsync(string serviceUniqueName, Guid stepId, int x, int y)
    {
        try
        {
            _ = await _agentEditorClient.MoveFlowStepAsync(new Ayborg.Gateway.V1.MoveFlowStepRequest
            {
                AgentUniqueName = serviceUniqueName,
                StepId = stepId.ToString(),
                X = x,
                Y = y
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error moving step");
            return false;
        }
    }

    /// <summary>
    /// Add link between ports asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryAddLinkAsync(string baseUrl, Guid sourcePortId, Guid targetPortId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/flow/links/{sourcePortId}/{targetPortId}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Removes the link asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRemoveLinkAsync(string baseUrl, Guid linkId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/flow/links/{linkId}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Gets the port for the given iteration.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    public async ValueTask<Port> GetPortAsync(string serviceUniqueName, Guid portId, Guid? iterationId = null)
    {
        var request = new Ayborg.Gateway.V1.GetFlowPortsRequest
        {
            AgentUniqueName = serviceUniqueName,
            IterationId = iterationId == null ? Guid.Empty.ToString() : iterationId.ToString()
        };
        request.PortIds.Add(portId.ToString());
        Ayborg.Gateway.V1.GetFlowPortsResponse response = await _agentEditorClient.GetFlowPortsAsync(request);
        Ayborg.Gateway.V1.Port? resultPort = response.Ports.FirstOrDefault();
        if (resultPort == null)
        {
            _logger.LogWarning("Could not find port with id {PortId} in iteration {IterationId}", portId, iterationId);
            return null!;
        }
        return RpcMapper.FromRpc(resultPort);
    }

    /// <summary>
    /// Try to set the port value asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    public async ValueTask<bool> TrySetPortValueAsync(string baseUrl, Port port)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/flow/ports")
        {
            Content = new StringContent(JsonConvert.SerializeObject(port), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}
