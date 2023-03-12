using System.Runtime.CompilerServices;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

internal sealed class FlowService : IFlowService
{
    private readonly ILogger<FlowService> _logger;
    private readonly IPluginsService _pluginsService;
    private readonly IEngineHost _runtimeHost;
    private readonly IRuntimeConverterService _runtimeConverterService;
    private readonly INotifyService _notifyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="pluginsService">The plugins service.</param>
    /// <param name="engineHost">The engine host.</param>
    /// <param name="runtimeConverterService">The runtime converter service.</param>
    /// <param name="notifyService">The notify service.</param>
    public FlowService(ILogger<FlowService> logger,
                        IPluginsService pluginsService,
                        IEngineHost engineHost,
                        IRuntimeConverterService runtimeConverterService,
                        INotifyService notifyService)
    {
        _logger = logger;
        _pluginsService = pluginsService;
        _runtimeHost = engineHost;
        _runtimeConverterService = runtimeConverterService;
        _notifyService = notifyService;
    }

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <returns>The steps.</returns>
    public IEnumerable<IStepProxy> GetSteps()
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
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
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
            return Enumerable.Empty<PortLink>();
        }

        return _runtimeHost.ActiveProject.Links;
    }

    /// <summary>
    /// Gets the port.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <returns></returns>
    public IPort GetPort(Guid portId)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
            return null!;
        }

        IStepProxy? targetStep = _runtimeHost.ActiveProject.Steps.FirstOrDefault(s => s.Ports.Any(p => p.Id == portId));
        if (targetStep == null)
        {
            _logger.LogTrace(new EventId((int)EventLogType.ProjectState), "Port with id '{portId}' not found. Already removed.", portId);
            return null!;
        }

        return targetStep.Ports.First(p => p.Id == portId);
    }

    /// <summary>
    /// Add step asynchronous.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async ValueTask<IStepProxy> AddStepAsync(Guid stepId, int x, int y)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
            return null!;
        }

        IStepProxy pluginProxy = _pluginsService.Find(stepId);

        if (pluginProxy == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Step with id '{stepId}' not found.", stepId);
            return null!;
        }

        IStepProxy stepProxy = _pluginsService.CreateInstance(pluginProxy.StepBody);
        stepProxy.X = x;
        stepProxy.Y = y;

        _runtimeHost.ActiveProject.Steps.Add(stepProxy);
        await _notifyService.SendAutomationFlowChangedAsync(new SDK.Communication.gRPC.Models.AgentAutomationFlowChangeArgs
        {
            AddedSteps = new[] { stepProxy.Id.ToString() }
        });

        _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Step [{stepName}] added.", stepProxy.Name);
        return stepProxy;
    }

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    public async ValueTask<bool> TryRemoveStepAsync(Guid stepId)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
            return false;
        }

        SDK.Projects.Project project = _runtimeHost.ActiveProject;
        IStepProxy? step = project.Steps.FirstOrDefault(s => s.Id == stepId);
        if (step == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Step with id '{stepId}' not found.", stepId);
            return false;
        }

        project.Steps.Remove(step);
        var removedLinks = new List<PortLink>();
        foreach (PortLink link in step.Links)
        {
            foreach (IStepProxy? linkedStep in project.Steps.Where(s => s.Links.Contains(link)))
            {
                foreach (IPort? linkedPort in linkedStep.Ports.Where(p => p.Id == link.SourceId || p.Id == link.TargetId))
                {
                    linkedPort.Disconnect();
                }

                linkedStep.Links.Remove(link);
                removedLinks.Add(link);
                if (project.Links.Contains(link))
                {
                    project.Links.Remove(link);
                }
            }
        }

        step.Dispose();
        await _notifyService.SendAutomationFlowChangedAsync(new SDK.Communication.gRPC.Models.AgentAutomationFlowChangeArgs
        {
            RemovedSteps = new[] { step.Id.ToString() },
            RemovedLinks = removedLinks.Select(l => l.Id.ToString()).ToArray()
        });

        _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Step [{stepName}] removed.", step.Name);
        return true;
    }

    /// <summary>
    /// Moves the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryMoveStepAsync(Guid stepId, int x, int y)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
            return false;
        }

        IStepProxy? step = _runtimeHost.ActiveProject.Steps.FirstOrDefault(s => s.Id == stepId);
        if (step == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Step with id '{stepId}' not found.", stepId);
            return false;
        }
        step.X = x;
        step.Y = y;

        await _notifyService.SendAutomationFlowChangedAsync(new SDK.Communication.gRPC.Models.AgentAutomationFlowChangeArgs
        {
            ChangedSteps = new[] { step.Id.ToString() }
        });

        return true;
    }

    /// <summary>
    /// Link ports together.
    /// </summary>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    public async ValueTask<PortLink> LinkPortsAsync(Guid sourcePortId, Guid targetPortId)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
            return null!;
        }

        IPort? sourcePort = null;
        IPort? targetPort = null;
        IStepProxy? sourceStep = null;
        IStepProxy? targetStep = null;
        foreach (IStepProxy step in _runtimeHost.ActiveProject.Steps)
        {
            IPort? sp = step.Ports.FirstOrDefault(p => p.Id == sourcePortId);
            IPort? tp = step.Ports.FirstOrDefault(p => p.Id == targetPortId);

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

        if (!ValidatePortLinkResult(sourcePortId, targetPortId, sourcePort!, targetPort!))
        {
            return null!;
        }

        if (!ValidateStepLinkResult(sourcePortId, targetPortId, sourceStep!, targetStep!))
        {
            return null!;
        }

        if (sourceStep!.Links.Any(x => x.SourceId == sourcePortId && x.TargetId == targetPortId)
            || targetStep!.Links.Any(x => x.SourceId == sourcePortId && x.TargetId == targetPortId))
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Ports with ids [{sourcePortName}] and [{targetPortName}] are already linked.", sourcePort!.Name, targetPort!.Name);
            return null!;
        }

        var link = new PortLink(sourcePort!, targetPort!);
        targetPort!.Connect(link);
        sourcePort!.Connect(link);
        sourceStep.Links.Add(link);
        targetStep.Links.Add(link);
        _runtimeHost.ActiveProject.Links.Add(link);

        await _notifyService.SendAutomationFlowChangedAsync(new SDK.Communication.gRPC.Models.AgentAutomationFlowChangeArgs
        {
            AddedLinks = new[] { link.Id.ToString() }
        });

        _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Port linked [{sourcePortName}] to [{targetPortName}].", sourcePort.Name, targetPort.Name);
        return link;
    }

    /// <summary>
    /// Unlink ports.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryUnlinkPortsAsync(Guid linkId)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
            return false;
        }

        IEnumerable<IStepProxy> steps = _runtimeHost.ActiveProject.Steps.Where(s => s.Links.Any(l => l.Id == linkId));
        if (!steps.Any())
        {
            _logger.LogTrace(new EventId((int)EventLogType.ProjectState), "Link with id '{linkId}' not found. Already removed.", linkId);
            return true;
        }

        PortLink link = steps.First().Links.First(l => l.Id == linkId);
        IPort sourcePort = link.Source;
        IPort targetPort = link.Target;
        sourcePort.Disconnect();
        targetPort.Disconnect();

        foreach (IStepProxy? step in steps)
        {
            step.Links.Remove(link);
        }

        _runtimeHost.ActiveProject.Links.Remove(link);

        await _notifyService.SendAutomationFlowChangedAsync(new SDK.Communication.gRPC.Models.AgentAutomationFlowChangeArgs
        {
            RemovedLinks = new[] { link.Id.ToString() }
        });

        _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Port unlinked [{sourcePortName}] from [{targetPortName}].", sourcePort.Name, targetPort.Name);
        return true;
    }

    /// <summary>
    /// Updates the port value.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryUpdatePortValueAsync(Guid portId, object value)
    {
        if (_runtimeHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
            return false;
        }

        IStepProxy? targetStep = _runtimeHost.ActiveProject.Steps.FirstOrDefault(s => s.Ports.Any(p => p.Id == portId));
        if (targetStep == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Port with id [{portId}] not found.", portId);
            return false;
        }

        IPort port = targetStep.Ports.First(p => p.Id == portId);

        await _notifyService.SendAutomationFlowChangedAsync(new SDK.Communication.gRPC.Models.AgentAutomationFlowChangeArgs
        {
            ChangedPorts = new[] { port.Id.ToString() }
        });

        if (await _runtimeConverterService.TryUpdatePortValueAsync(port, value))
        {
            _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Port [{portName}] updated.", port.Name);
            return true;
        }
        else
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Port [{portName}] update failed.", port.Name);
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ValidatePortLinkResult(Guid sourcePortId, Guid targetPortId, IPort sourcePort, IPort targetPort)
    {
        if (sourcePort == null || targetPort == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Ports with ids [{sourcePortId}] and/or [{targetPortId}] not found.", sourcePortId, targetPortId);
            return false;
        }

        if (sourcePort.Direction != PortDirection.Output || targetPort.Direction != PortDirection.Input)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Ports with ids [{sourcePortId}] and [{targetPortId}] are not compatible.", sourcePortId, targetPortId);
            return false;
        }

        if (!PortConverter.IsConvertable(sourcePort, targetPort))
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Ports with ids [{sourcePortId}] and [{targetPortId}] are not convertable.", sourcePortId, targetPortId);
            return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ValidateStepLinkResult(Guid sourcePortId, Guid targetPortId, IStepProxy sourceStep, IStepProxy targetStep)
    {
        if (sourceStep!.Id == targetStep!.Id)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Ports with ids [{sourcePortId}] and [{targetPortId}] are in the same step.", sourcePortId, targetPortId);
            return false;
        }

        return true;
    }
}
