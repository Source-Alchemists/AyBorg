using System.Runtime.CompilerServices;
using AyBorg.Data.Agent;
using AyBorg.Data.Mapper;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;

[assembly: InternalsVisibleTo("AyBorg.Agent.Tests")]
namespace AyBorg.Agent.Services;

internal sealed class RuntimeConverterService : IRuntimeConverterService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<RuntimeConverterService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPluginsService _pluginsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeConverterService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="pluginsService">The plugins service.</param>
    public RuntimeConverterService(ILoggerFactory loggerFactory, ILogger<RuntimeConverterService> logger, IServiceProvider serviceProvider, IPluginsService pluginsService)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _pluginsService = pluginsService;
    }

    /// <summary>
    /// Converts the specified project.
    /// </summary>
    /// <param name="projectRecord">The project.</param>
    /// <returns></returns>
    public async ValueTask<Project> ConvertAsync(ProjectRecord projectRecord)
    {
        var project = new Project
        {
            Meta = new ProjectMeta
            {
                Id = projectRecord.Meta.Id,
                Name = projectRecord.Meta.Name,
                State = projectRecord.Meta.State,
            },
            Settings = new ProjectSettings
            {
                IsForceResultCommunicationEnabled = projectRecord.Settings.IsForceResultCommunicationEnabled
            },
            // First, we need to convert all the steps
            Steps = await ConvertStepsAsync(projectRecord.Steps)
        };
        // Then, we need to convert all the links
        ConvertLinks(projectRecord, project);
        return project;
    }

    /// <summary>
    /// Updates the port value.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryUpdatePortValueAsync(IPort port, object value)
    {
        IPortMapper mapper = PortMapperFactory.CreateMapper(port);
        mapper.Update(port, value);
        return await ValueTask.FromResult(true);
    }

    /// <summary>
    /// Updates the port values.
    /// </summary>
    /// <param name="ports">The ports.</param>
    /// <param name="portRecords">The port records.</param>
    public async ValueTask UpdateValuesAsync(IEnumerable<IPort> ports, IEnumerable<PortRecord> portRecords)
    {
        foreach (IPort port in ports)
        {
            PortRecord? portRecord = portRecords.FirstOrDefault(x => x.Name == port.Name && x.Direction == port.Direction);
            if (portRecord == null)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Port record {port.Name} not found! Will use default value.", port.Name);
                continue;
            }

            port.SetId(portRecord.Id);

            await TryUpdatePortValueAsync(port, portRecord.Value);
        }
    }

    private async ValueTask<ICollection<IStepProxy>> ConvertStepsAsync(ICollection<StepRecord> stepRecords)
    {
        var steps = new List<IStepProxy>();
        foreach (StepRecord stepRecord in stepRecords)
        {
            try
            {
                IStepProxy step = await ConvertStepAsync(stepRecord);
                steps.Add(step);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(new EventId((int)EventLogType.Engine), ex, "Step {StepName} with type {TypeName} not found", stepRecord.Name, stepRecord.MetaInfo.TypeName);
            }
        }

        return steps;
    }

    private async ValueTask<IStepProxy> ConvertStepAsync(StepRecord stepRecord)
    {
        IStepProxy proxyInstance = _pluginsService.Find(stepRecord) ?? throw new KeyNotFoundException(nameof(stepRecord.MetaInfo.TypeName));
        if (ActivatorUtilities.CreateInstance(_serviceProvider, proxyInstance.StepBody.GetType()) is IStepBody stepBody)
        {
            var stepProxy = new StepProxy(_loggerFactory.CreateLogger<StepProxy>(), stepBody, stepRecord.X, stepRecord.Y)
            {
                Id = stepRecord.Id,
                Name = stepRecord.Name
            };
            await UpdateValuesAsync(stepProxy.Ports, stepRecord.Ports);
            return stepProxy;
        }
        else
        {
            throw new InvalidOperationException($"{stepRecord.MetaInfo.TypeName} is not a step body");
        }
    }

    private void ConvertLinks(ProjectRecord projectRecord, Project project)
    {
        var linkRecordHashes = new HashSet<LinkRecord>();
        foreach (LinkRecord linkRecord in projectRecord.Links)
        {
            if (linkRecordHashes.Contains(linkRecord))
            {
                continue;
            }

            linkRecordHashes.Add(linkRecord);
            try
            {
                IStepProxy sourceStep = project.Steps.First(s => s.Ports.Any(p => p.Id == linkRecord.SourceId));
                IStepProxy targetStep = project.Steps.First(s => s.Ports.Any(p => p.Id == linkRecord.TargetId));
                IPort sourcePort = sourceStep.Ports.First(p => p.Id == linkRecord.SourceId);
                IPort targetPort = targetStep.Ports.First(p => p.Id == linkRecord.TargetId);
                var link = new PortLink(linkRecord.Id, sourcePort, targetPort);
                sourcePort.Connect(link);
                targetPort.Connect(link);
                sourceStep.Links.Add(link);
                targetStep.Links.Add(link);
                project.Links.Add(link);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert link {linkRecord.Id}.", linkRecord.Id);
            }
        }
    }
}
