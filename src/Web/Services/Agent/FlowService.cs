using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.Web.Services.AppState;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public class FlowService : IFlowService
{
    private readonly ILogger<FlowService> _logger;
    private readonly IStateService _stateService;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly Ayborg.Gateway.Agent.V1.AgentEditor.AgentEditorClient _agentEditorClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="agentCacheService">The agent cache service.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public FlowService(ILogger<FlowService> logger,
                        IStateService stateService,
                        IAuthorizationHeaderUtilService authorizationHeaderUtilService,
                        Ayborg.Gateway.Agent.V1.AgentEditor.AgentEditorClient agentEditorClient)
    {
        _logger = logger;
        _stateService = stateService;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
        _agentEditorClient = agentEditorClient;
    }

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <returns>The steps.</returns>
    public async ValueTask<IEnumerable<Step>> GetStepsAsync()
    {
        Ayborg.Gateway.Agent.V1.GetFlowStepsResponse response = await _agentEditorClient.GetFlowStepsAsync(new Ayborg.Gateway.Agent.V1.GetFlowStepsRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName
        });
        var result = new List<Step>();
        foreach (Ayborg.Gateway.Agent.V1.Step? s in response.Steps)
        {
            result.Add(RpcMapper.FromRpc(s));
        }

        return result;
    }

    /// <summary>
    /// Gets the links.
    /// </summary>
    /// <returns>The links.</returns>
    public async ValueTask<IEnumerable<Link>> GetLinksAsync()
    {
        Ayborg.Gateway.Agent.V1.GetFlowLinksResponse response = await _agentEditorClient.GetFlowLinksAsync(new Ayborg.Gateway.Agent.V1.GetFlowLinksRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName
        });
        var result = new List<Link>();
        foreach (Ayborg.Gateway.Agent.V1.Link? l in response.Links)
        {
            result.Add(RpcMapper.FromRpc(l));
        }

        return result;
    }

    /// <summary>
    /// Adds the step asynchronous.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async ValueTask<Step> AddStepAsync(Guid stepId, int x, int y)
    {
        Ayborg.Gateway.Agent.V1.AddFlowStepResponse response = await _agentEditorClient.AddFlowStepAsync(new Ayborg.Gateway.Agent.V1.AddFlowStepRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName,
            StepId = stepId.ToString(),
            X = x,
            Y = y
        });

        return RpcMapper.FromRpc(response.Step);
    }

    /// <summary>
    /// Removes the step asynchronous.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRemoveStepAsync(Guid stepId)
    {
        try
        {
            _ = await _agentEditorClient.DeleteFlowStepAsync(new Ayborg.Gateway.Agent.V1.DeleteFlowStepRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
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
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryMoveStepAsync(Guid stepId, int x, int y)
    {
        try
        {
            _ = await _agentEditorClient.MoveFlowStepAsync(new Ayborg.Gateway.Agent.V1.MoveFlowStepRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
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
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryAddLinkAsync(Guid sourcePortId, Guid targetPortId)
    {
        try
        {
            _ = await _agentEditorClient.LinkFlowPortsAsync(new Ayborg.Gateway.Agent.V1.LinkFlowPortsRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                SourceId = sourcePortId.ToString(),
                TargetId = targetPortId.ToString()
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error linking ports");
            return false;
        }
    }

    /// <summary>
    /// Removes the link asynchronous.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRemoveLinkAsync(Guid linkId)
    {
        try
        {
            _ = await _agentEditorClient.LinkFlowPortsAsync(new Ayborg.Gateway.Agent.V1.LinkFlowPortsRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                SourceId = linkId.ToString(),
                TargetId = string.Empty
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error linking ports");
            return false;
        }
    }

    /// <summary>
    /// Gets the port for the given iteration.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    public async ValueTask<Port> GetPortAsync(Guid portId, Guid? iterationId = null)
    {
        var request = new Ayborg.Gateway.Agent.V1.GetFlowPortsRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName,
            IterationId = iterationId == null ? Guid.Empty.ToString() : iterationId.ToString()
        };
        request.PortIds.Add(portId.ToString());
        Ayborg.Gateway.Agent.V1.GetFlowPortsResponse response = await _agentEditorClient.GetFlowPortsAsync(request);
        Ayborg.Gateway.Agent.V1.Port? resultPort = response.Ports.FirstOrDefault();
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
    /// <param name="port">The port.</param>
    /// <returns></returns>
    public async ValueTask<bool> TrySetPortValueAsync(Port port)
    {
        try
        {
            _ = await _agentEditorClient.UpdateFlowPortAsync(new Ayborg.Gateway.Agent.V1.UpdateFlowPortRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                Port = RpcMapper.ToRpc(port)
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Error setting port value");
            return false;
        }
    }
}
