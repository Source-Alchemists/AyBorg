using System.Collections.Immutable;
using System.Reflection;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;
using McMaster.NETCore.Plugins;

namespace AyBorg.Agent.Services;
internal sealed class PluginsService : IPluginsService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<PluginsService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private ImmutableList<IStepProxy> _stepPlugins = ImmutableList.Create<IStepProxy>();
    private ImmutableList<IDeviceProviderProxy> _deviceProviderPlugins = ImmutableList.Create<IDeviceProviderProxy>();

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <value>
    /// The steps.
    /// </value>
    public IReadOnlyCollection<IStepProxy> Steps => _stepPlugins;

    /// <summary>
    /// Gets the device providers.
    /// </summary>
    public IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders => _deviceProviderPlugins;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="configuration">The configuration.</param>
    public PluginsService(ILoggerFactory loggerFactory, ILogger<PluginsService> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    public async ValueTask LoadAsync()
    {
        _stepPlugins = _stepPlugins.Clear();
        _deviceProviderPlugins = _deviceProviderPlugins.Clear();

        try
        {
            string? configFolder = _configuration.GetValue<string>("AyBorg:Plugins:Folder");
            if (configFolder == null)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Engine), "No plugin folder specified in configuration. (Hint: AyBorg:Plugins:Folder)");
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
                    string assemblyPath = $"{Path.Combine(pd, dllName)}";
                    Assembly assembly = PluginLoader.CreateFromAssemblyFile(assemblyPath, c => c.PreferSharedTypes = true)
                                                    .LoadDefaultAssembly();

                    await LoadPluginsAsync(assembly);
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

    public IDeviceProviderProxy FindDeviceProvider(PluginMetaInfoRecord pluginMetaInfo)
    {
        IEnumerable<IDeviceProviderProxy> matchingProviders = _deviceProviderPlugins.Where(p => p.MetaInfo.AssemblyName.Equals(pluginMetaInfo.AssemblyName, StringComparison.InvariantCultureIgnoreCase));
        return matchingProviders.Single(p => p.MetaInfo.TypeName.Equals(pluginMetaInfo.TypeName, StringComparison.InvariantCultureIgnoreCase));
    }

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

        return new StepProxy(_loggerFactory.CreateLogger<StepProxy>(), newInstance);
    }

    private async ValueTask LoadPluginsAsync(Assembly assembly)
    {
        Type stepBodyType = typeof(IStepBody);
        Type deviceManagerType = typeof(IDeviceProvider);

        // Load device provider plugins
        IEnumerable<Type> deviceProviderPlugins = assembly.GetTypes().Where(p => deviceManagerType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
        await LoadDeviceProvidersAsync(deviceProviderPlugins);

        // Load step plugins
        IEnumerable<Type> stepPlugins = assembly.GetTypes().Where(p => stepBodyType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
        LoadSteps(stepPlugins);
    }

    private void LoadSteps(IEnumerable<Type> stepPlugins)
    {
        if (!stepPlugins.Any())
        {
            return;
        }

        foreach (Type sp in stepPlugins)
        {
            if (ActivatorUtilities.CreateInstance(_serviceProvider, sp) is IStepBody si)
            {
                _stepPlugins = _stepPlugins.Add(new StepProxy(_loggerFactory.CreateLogger<StepProxy>(), si));
                _logger.LogTrace((int)EventLogType.Plugin, "Added step plugin '{si.GetType.Name}'.", si.GetType().Name);
            }
        }
    }

    private async ValueTask LoadDeviceProvidersAsync(IEnumerable<Type> deviceProviderPlugins)
    {
        if (!deviceProviderPlugins.Any())
        {
            return;
        }

        foreach (Type dm in deviceProviderPlugins)
        {
            if (ActivatorUtilities.CreateInstance(_serviceProvider, dm) is IDeviceProvider di)
            {
                var deviceProviderProxy = new DeviceProviderProxy(_loggerFactory, _loggerFactory.CreateLogger<DeviceProviderProxy>(), di);
                if (!await deviceProviderProxy.TryInitializeAsync())
                {
                    _logger.LogWarning((int)EventLogType.Plugin, "Device provider plugin '{di.GetType.Name}' could not be initialized.", di.GetType().Name);
                    continue;
                }

                _deviceProviderPlugins = _deviceProviderPlugins.Add(deviceProviderProxy);
                _logger.LogTrace((int)EventLogType.Plugin, "Added device provider plugin '{di.GetType.Name}'.", di.GetType().Name);
            }
        }
    }

    private static bool IsSameType(Type type, PluginMetaInfoRecord metaInfo) => type.Name == metaInfo.TypeName && type.Assembly.GetName().Name == metaInfo.AssemblyName;
}
