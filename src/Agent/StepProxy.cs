using System.Diagnostics;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public sealed class StepProxy : IStepProxy
{
    private readonly ILogger<StepProxy> _logger;
    private readonly Stopwatch _stopwatch = new();
    private bool _lastResult = false;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="StepProxy"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stepBody">The step body.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    public StepProxy(ILogger<StepProxy> logger, IStepBody stepBody, int x = -1, int y = -1)
    {
        _logger = logger;
        StepBody = stepBody;
        Ports = stepBody.Ports;
        Name = stepBody.Name;
        Categories = stepBody.Categories;
        X = x;
        Y = y;

        string typeName = stepBody.GetType().Name;
        System.Reflection.Assembly assembly = stepBody.GetType().Assembly;
        System.Reflection.AssemblyName? assemblyName = assembly?.GetName();

        MetaInfo = new PluginMetaInfo
        {
            TypeName = typeName,
            AssemblyName = assemblyName!.Name!,
            AssemblyVersion = assemblyName!.Version!.ToString()
        };
    }

    /// <summary>
    /// Called when the step is executed.
    /// </summary>
    public event EventHandler<bool>? Completed;

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the categories.
    /// </summary>
    public IReadOnlyCollection<string> Categories { get; }

    /// <summary>
    /// Gets the links.
    /// </summary>
    public IList<PortLink> Links { get; } = new List<PortLink>();

    /// <summary>
    /// Gets the ports.
    /// </summary>
    public IEnumerable<IPort> Ports { get; }

    /// <summary>
    /// Gets the step body.
    /// </summary>
    public IStepBody StepBody { get; }

    /// <summary>
    /// Gets or sets the x.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the y.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the meta information.
    /// </summary>
    public PluginMetaInfo MetaInfo { get; }

    /// <summary>
    /// Gets the iteration identifier the step was execution last in.
    /// </summary>
    public Guid IterationId { get; private set; } = Guid.Empty;

    /// <summary>
    /// Gets the execution time in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; private set; }

    /// <summary>
    /// Executes the step.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryRunAsync(Guid iterationId, CancellationToken cancellationToken)
    {
        if (IterationId == iterationId)
        {
            // Already executed in this iteration.
            return _lastResult;
        }

        _stopwatch.Restart();
        bool result = false;
        try
        {
            result = await StepBody.TryRunAsync(cancellationToken);
        }
        finally
        {
            _lastResult = result;
            // Updating the iteration id after the step has been executed.
            // This is used to determine if the step has been executed in the current iteration.
            IterationId = iterationId;
            _stopwatch.Stop();
            ExecutionTimeMs = _stopwatch.ElapsedMilliseconds;
            Completed?.Invoke(this, result);
        }
        return result;
    }

    /// <summary>
    /// Initializes the step.
    /// </summary>
    public async ValueTask<bool> TryInitializeAsync()
    {
        try
        {
            if (StepBody is IInitializable initializable)
            {
                await initializable.OnInitializeAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning((int)EventLogType.Plugin, ex, "Failed to initialize step {Name}", Name);
            return false;
        }
    }

    public void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            if (StepBody is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
