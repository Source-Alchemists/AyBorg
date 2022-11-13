using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.ImageProcessing.Shapes;
using AyBorg.SDK.Projects;

[assembly: InternalsVisibleTo("AyBorg.Agent.Tests")]
namespace AyBorg.Agent.Services;

internal sealed class RuntimeConverterService : IRuntimeConverterService
{
    private readonly ILogger<RuntimeConverterService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPluginsService _pluginsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeConverterService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="pluginsService">The plugins service.</param>
    public RuntimeConverterService(ILogger<RuntimeConverterService> logger, IServiceProvider serviceProvider, IPluginsService pluginsService)
    {
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
        switch (port)
        {
            case NumericPort numericPort:
                return UpdateNumericPortValue(numericPort, value);
            case StringPort stringPort: // Also covers FolderPort
                return UpdateStringPortValue(stringPort, value);
            case BooleanPort booleanPort:
                return UpdateBooleanPortValue(booleanPort, value);
            case EnumPort enumPort:
                return UpdateEnumPortValue(enumPort, value);
            case RectanglePort rectanglePort:
                return UpdateRectanglePortValue(rectanglePort, value);
            case ImagePort imagePort:
                return UpdateImagePortValue(imagePort);
        }

        _logger.LogWarning("Port type {PortType} is not supported", port.GetType().Name);
        return await ValueTask.FromResult(false);
    }

    private static bool UpdateNumericPortValue(NumericPort port, object value)
    {
        if (value is JsonElement jsonElement)
        {
            port.Value = jsonElement.GetDouble();
        }
        else
        {
            port.Value = Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        return true;
    }

    private static bool UpdateStringPortValue(StringPort port, object value)
    {
        if (value is JsonElement jsonElement)
        {
            port.Value = jsonElement.GetString() ?? string.Empty;
        }
        else
        {
            port.Value = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        return true;
    }

    private static bool UpdateBooleanPortValue(BooleanPort port, object value)
    {
        if (value is JsonElement jsonElement)
        {
            port.Value = jsonElement.GetBoolean();
        }
        else
        {
            port.Value = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        return true;
    }

    private static bool UpdateEnumPortValue(EnumPort port, object value)
    {
        var record = JsonSerializer.Deserialize<EnumRecord>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        port.Value = (Enum)Enum.Parse(port.Value.GetType(), record.Name);

        return true;
    }

    private static bool UpdateImagePortValue(ImagePort port)
    {
        port.Value = null!; // Nothing to do, images will be created at runtime.
        return true;
    }

    private static bool UpdateRectanglePortValue(RectanglePort port, object value)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var record = JsonSerializer.Deserialize<RectangleRecord>(value.ToString()!, options);
        if (record == null)
        {
            return false;
        }
        port.Value = new Rectangle(record.X, record.Y, record.Width, record.Height);

        return true;
    }

    private async ValueTask<ICollection<IStepProxy>> ConvertStepsAsync(ICollection<StepRecord> stepRecords)
    {
        var steps = new List<IStepProxy>();
        foreach (var stepRecord in stepRecords)
        {
            var step = await ConvertStepAsync(stepRecord);
            steps.Add(step);
        }

        return steps;
    }

    private async ValueTask<IStepProxy> ConvertStepAsync(StepRecord stepRecord)
    {
        var proxyInstance = _pluginsService.Find(stepRecord);
        if (proxyInstance == null) throw new KeyNotFoundException(nameof(stepRecord.MetaInfo.TypeName));

        if (ActivatorUtilities.CreateInstance(_serviceProvider, proxyInstance.StepBody.GetType()) is IStepBody stepBody)
        {
            var stepProxy = new StepProxy(stepBody, stepRecord.X, stepRecord.Y)
            {
                Id = stepRecord.Id,
                Name = stepRecord.Name
            };
            await ChangePortsValues(stepProxy.Ports, stepRecord.Ports);
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
        foreach (var linkRecord in projectRecord.Links)
        {
            if (linkRecordHashes.Contains(linkRecord))
            {
                continue;
            }

            linkRecordHashes.Add(linkRecord);
            try
            {
                var sourceStep = project.Steps.First(s => s.Ports.Any(p => p.Id == linkRecord.SourceId));
                var targetStep = project.Steps.First(s => s.Ports.Any(p => p.Id == linkRecord.TargetId));
                var sourcePort = sourceStep.Ports.First(p => p.Id == linkRecord.SourceId);
                var targetPort = targetStep.Ports.First(p => p.Id == linkRecord.TargetId);
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

    private async ValueTask ChangePortsValues(IEnumerable<IPort> ports, IEnumerable<PortRecord> portRecords)
    {
        foreach (var port in ports)
        {
            var portRecord = portRecords.FirstOrDefault(x => x.Name == port.Name && x.Direction == port.Direction);
            if (portRecord == null)
            {
                _logger.LogWarning("Port record {port.Name} not found! Will use default value.", port.Name);
                continue;
            }

            port.SetId(portRecord.Id);

            await TryUpdatePortValueAsync(port, portRecord.Value);
        }
    }
}