using Autodroid.SDK.Common;
using Autodroid.SDK.Data.DAL;
using System.Reflection;

namespace Autodroid.Agent.Services;
internal sealed class PluginsService : IPluginsService
{
    private readonly ILogger<PluginsService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IList<IStepProxy> _stepPlugins = new List<IStepProxy>();

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <value>
    /// The steps.
    /// </value>
    public IEnumerable<IStepProxy> Steps => _stepPlugins;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="configuration">The configuration.</param>
    public PluginsService(ILogger<PluginsService> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    public void Load()
    {
        _stepPlugins.Clear();
        try
        {
            var pluginsDir = Path.GetFullPath(_configuration.GetValue<string>("Autodroid:Plugins:Folder"));

            _logger.LogTrace("Loading plugins in '{pluginsDir}' ...", pluginsDir);

            foreach (var pd in Directory.EnumerateDirectories(pluginsDir))
            {
                var dir = Path.GetFileName(pd);
                var dllName = $"{dir}.dll";

                if (Directory.EnumerateFiles(pd).Where(x => Path.GetFileName(x).Equals(dllName)).Count() == 1)
                {
                    _logger.LogTrace("Detected directory '{pd}'.", pd);
                    var assembly = Assembly.LoadFile($"{Path.Combine(pd, dllName)}");
                    if (!TryLoadPlugins(assembly))
                    {
                        _logger.LogTrace("No plugins detected.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugins.");
            throw;
        }
    }

    /// <summary>
    /// Find plugin instance by step record.
    /// </summary>
    /// <param name="stepRecord">The step record.</param>
    /// <returns>Instance, else null.</returns>
    public IStepProxy Find(StepRecord stepRecord) => _stepPlugins.FirstOrDefault(x => IsSameType(x.StepBody.GetType(), stepRecord.MetaInfo))!;

    /// <summary>
    /// Finds the specified step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns>Instance, else null.</returns>
    public IStepProxy Find(Guid stepId) => _stepPlugins.FirstOrDefault(x => x.Id.Equals(stepId))!;

    /// <summary>
    /// Creates new instance of step.
    /// </summary>
    /// <param name="stepBody">The step body.</param>
    /// <returns></returns>
    public IStepProxy CreateInstance(IStepBody stepBody)
    {
        if (ActivatorUtilities.CreateInstance(_serviceProvider, stepBody.GetType()) is not IStepBody newInstance)
        {
            throw new InvalidOperationException($"Step body '{stepBody.GetType().FullName}' is not a valid step body.");
        }

        return new StepProxy(newInstance);
    }

    private bool TryLoadPlugins(Assembly assembly)
    {
        var stepBodyType = typeof(IStepBody);

        var stepPlugins = assembly.GetTypes().Where(p => stepBodyType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
        if (stepPlugins.Any())
        {
            foreach (var sp in stepPlugins)
            {
                if (ActivatorUtilities.CreateInstance(_serviceProvider, sp) is IStepBody si)
                {
                    _stepPlugins.Add(new StepProxy(si));
                    _logger.LogTrace("Added step plugin '{si.GetType.Name}'.", si.GetType().Name);
                }
            }
            return true;
        }

        return false;
    }

    private static bool IsSameType(Type type, PluginMetaInfo metaInfo) => type.Name == metaInfo.TypeName && type.Assembly.GetName().Name == metaInfo.AssemblyName;
}
