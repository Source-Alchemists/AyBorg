using Atomy.Agent.Hubs;
using Atomy.SDK;
using Atomy.SDK.Ports;

namespace Atomy.Agent.Services;

public class FlowService : IFlowService
{
    private readonly ILogger<FlowService> _logger;
    private readonly IPluginsService _pluginsService;
    private readonly IEngineHost _runtimeHost;
    private readonly IFlowHub _flowHub;
    private readonly IRuntimeConverterService _runtimeConverterService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="pluginsService">The plugins service.</param>
    /// <param name="runtimeHost">The runtime host.</param>
    /// <param name="flowHub">The flow hub.</param>
    /// <param name="runtimeConverterService">The runtime converter service.</param>
    public FlowService(ILogger<FlowService> logger,
                        IPluginsService pluginsService,
                        IEngineHost runtimeHost,
                        IFlowHub flowHub,
                        IRuntimeConverterService runtimeConverterService)
    {
        _logger = logger;
        _pluginsService = pluginsService;
        _runtimeHost = runtimeHost;
        _flowHub = flowHub;
        _runtimeConverterService = runtimeConverterService;
    }

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <returns>The steps.</returns>
    public IEnumerable<IStepProxy> GetSteps()
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return Enumerable.Empty<IStepProxy>();
        }

        return _runtimeHost.ActiveProject.Steps;
    }

    /// <summary>
    /// Gets the links.
    /// </summary>
    public IEnumerable<PortLink> GetLinks()
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return Enumerable.Empty<PortLink>();
        }

        return _runtimeHost.ActiveProject.Links;
    }

    /// <summary>
    /// Add step asynchronous.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async Task<IStepProxy> AddStepAsync(Guid stepId, int x, int y)
    {
        var pluginProxy = _pluginsService.Find(stepId);
        if(_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return null!;
        }

        if (pluginProxy == null)
        {
            _logger.LogWarning($"Step with id '{stepId}' not found.");
            return null!;
        }

        var stepProxy = _pluginsService.CreateInstance(pluginProxy.StepBody);
        stepProxy.X = x;
        stepProxy.Y = y;

        _runtimeHost.ActiveProject.Steps.Add(stepProxy);

        return await Task.FromResult(stepProxy);
    }

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    public async Task<bool> TryRemoveStepAsync(Guid stepId)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return false;
        }

        var project = _runtimeHost.ActiveProject;
        var step = project.Steps.FirstOrDefault(s => s.Id == stepId);
        if(step == null)
        {
            _logger.LogWarning($"Step with id '{stepId}' not found.");
            return false;
        }

        project.Steps.Remove(step);

        var stepLinks = new List<PortLink>();
        foreach(var link in step.Links)
        {
            stepLinks.Add(link);
            if(project.Links.Contains(link))
            {
                project.Links.Remove(link);
            }
        }

        foreach(var link in stepLinks) {
            foreach(var linkedStep in project.Steps.Where(s => s.Links.Contains(link)))
            {
                foreach(var linkedPort in linkedStep.Ports.Where(p => p.Id == link.SourceId || p.Id == link.TargetId))
                {
                    linkedPort.Disconnect();
                }
                
                linkedStep.Links.Remove(link);
            }
        }
        // ToDo: Dispose step if needed.
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Moves the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async Task<bool> TryMoveStepAsync(Guid stepId, int x, int y)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return false;
        }

        var step = _runtimeHost.ActiveProject.Steps.FirstOrDefault(s => s.Id == stepId);
        if (step == null)
        {
            _logger.LogWarning($"Step with id '{stepId}' not found.");
            return false;
        }
        step.X = x;
        step.Y = y;

        await Task.CompletedTask;
        return true;
    }

    /// <summary>
    /// Link ports together.
    /// </summary>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    public async Task<PortLink> LinkPortsAsync(Guid sourcePortId, Guid targetPortId)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return null!;
        }

        IPort? sourcePort = null;
        IPort? targetPort = null;
        IStepProxy? sourceStep = null;
        IStepProxy? targetStep = null;
        foreach (var step in _runtimeHost.ActiveProject.Steps)
        {
            var sp = step.Ports.FirstOrDefault(p => p.Id == sourcePortId);
            var tp = step.Ports.FirstOrDefault(p => p.Id == targetPortId);

            if (sp != null)
            {
                sourcePort = sp;
                sourceStep = step;
            }
            if (tp != null)
            {
                targetPort = tp;
                targetStep = step;
            }
        }

        if (sourcePort == null || targetPort == null)
        {
            _logger.LogWarning($"Ports with ids '{sourcePortId}' and/or '{targetPortId}' not found.");
            return null!;
        }

        if (sourceStep!.Id == targetStep!.Id)
        {
            _logger.LogWarning($"Ports with ids '{sourcePortId}' and '{targetPortId}' are in the same step.");
            return null!;
        }

        if (sourcePort.Direction != PortDirection.Output || targetPort.Direction != PortDirection.Input)
        {
            _logger.LogWarning($"Ports with ids '{sourcePortId}' and '{targetPortId}' are not compatible.");
            return null!;
        }

        if (sourceStep.Links.Any(x => x.SourceId == sourcePortId && x.TargetId == targetPortId)
            || targetStep.Links.Any(x => x.SourceId == sourcePortId && x.TargetId == targetPortId))
        {
            _logger.LogWarning($"Ports with ids '{sourcePortId}' and '{targetPortId}' are already linked.");
            return null!;
        }

        if(!PortConverter.IsConvertable(sourcePort, targetPort))
        {
            _logger.LogWarning($"Ports with ids '{sourcePortId}' and '{targetPortId}' are not convertable.");
            return null!;
        }

        var link = new PortLink(sourcePort, targetPort);
        targetPort.Connect(link);
        sourcePort.Connect(link);
        sourceStep.Links.Add(link);
        targetStep.Links.Add(link);
        _runtimeHost.ActiveProject.Links.Add(link);
        await _flowHub.SendLinkChangedAsync(link);
        return await Task.FromResult<PortLink>(link);
    }

    /// <summary>
    /// Unlink ports.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    public async Task<bool> TryUnlinkPortsAsync(Guid linkId)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return false;
        }

        var steps = _runtimeHost.ActiveProject.Steps.Where(s => s.Links.Any(l => l.Id == linkId));
        if(!steps.Any())
        {
            _logger.LogTrace($"Link with id '{linkId}' not found. Already removed.");
            return true;
        }

        var link = steps.First().Links.First(l => l.Id == linkId);
        var sourcePort = link.Source;
        var targetPort = link.Target;
        sourcePort.Disconnect();
        targetPort.Disconnect();

        foreach (var step in steps)
        {
            step.Links.Remove(link);
        }

        _runtimeHost.ActiveProject.Links.Remove(link);
        await _flowHub.SendLinkChangedAsync(link, true);
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Gets the port.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <returns></returns>
    public async Task<IPort> GetPortAsync(Guid portId)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return null!;
        }

        var targetStep = _runtimeHost.ActiveProject.Steps.FirstOrDefault(s => s.Ports.Any(p => p.Id == portId));
        if (targetStep == null)
        {
            _logger.LogTrace($"Port with id '{portId}' not found. Already removed.");
            return null!;
        }

        return await Task.FromResult(targetStep.Ports.First(p => p.Id == portId));
    }

    /// <summary>
    /// Updates the port value.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public async Task<bool> TryUpdatePortValueAsync(Guid portId, object value)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project found.");
            return false;
        }

        var targetStep = _runtimeHost.ActiveProject.Steps.FirstOrDefault(s => s.Ports.Any(p => p.Id == portId));
        if (targetStep == null)
        {
            _logger.LogWarning($"Port with id '{portId}' not found.");
            return false;
        }

        var port = targetStep.Ports.First(p => p.Id == portId);
        return await _runtimeConverterService.TryUpdatePortValueAsync(port, value);
    }
}