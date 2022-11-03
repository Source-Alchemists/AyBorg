using System.Diagnostics;
using Atomy.SDK;
using Atomy.SDK.Ports;

namespace Atomy.Agent;

public class StepProxy : IStepProxy
{
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private bool _lastResult = false;
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="StepProxy"/> class.
    /// </summary>
    /// <param name="stepBody">The step body.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    public StepProxy(IStepBody stepBody, int x = -1, int y = -1)
    {
        StepBody = stepBody;
        Ports = stepBody.Ports;
        Name = stepBody.DefaultName;
        X = x;
        Y = y;

        var typeName = stepBody.GetType().Name;
        var assembly = stepBody.GetType().Assembly;
        var assemblyName = assembly?.GetName();

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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    public async Task<bool> TryRunAsync(CancellationToken cancellationToken, Guid iterationId)
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
    public async Task InitializeAsync()
    {
        if (StepBody is IInitializable initializable)
        {
            await initializable.OnInitializeAsync();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (StepBody is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
