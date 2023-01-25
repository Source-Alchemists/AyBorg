using System.Reflection;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;
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
            string? configFolder = _configuration.GetValue<string>("AyBorg:Plugins:Folder");
            if (configFolder == null)
            {
                _logger.LogWarning("No plugin folder specified in configuration. (Hint: AyBorg:Plugins:Folder)");
                return;
            }

            string pluginsDir = Path.GetFullPath(configFolder);

            _logger.LogTrace("Loading plugins in '{pluginsDir}' ...", pluginsDir);

            foreach (string pd in Directory.EnumerateDirectories(pluginsDir))
            {
                string dir = Path.GetFileName(pd);
                string dllName = $"{dir}.dll";

                if (Directory.EnumerateFiles(pd).Where(x => Path.GetFileName(x).Equals(dllName)).Count() == 1)
                {
                    _logger.LogTrace("Detected directory '{pd}'.", pd);
                    // use load from assembly to load other dependencies from same folder
                    var assembly = Assembly.LoadFrom($"{Path.Combine(pd, dllName)}");
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
        Type stepBodyType = typeof(IStepBody);

        IEnumerable<Type> stepPlugins = assembly.GetTypes().Where(p => stepBodyType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
        if (stepPlugins.Any())
        {
            foreach (Type sp in stepPlugins)
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

    private static bool IsSameType(Type type, PluginMetaInfoRecord metaInfo) => type.Name == metaInfo.TypeName && type.Assembly.GetName().Name == metaInfo.AssemblyName;
}
