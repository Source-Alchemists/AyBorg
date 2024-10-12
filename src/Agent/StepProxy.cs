/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Diagnostics;
using AyBorg.Runtime;
using AyBorg.Types;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using SR = System.Reflection;

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
        SR.Assembly assembly = stepBody.GetType().Assembly;
        SR.AssemblyName? assemblyName = assembly?.GetName();

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
    /// Initializes the step before running it.
    /// </summary>
    public async ValueTask<bool> TryBeforeStartAsync()
    {
        try
        {
            if (StepBody is IBeforeStart beforeStartable)
            {
                await beforeStartable.BeforeStartAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning((int)EventLogType.Plugin, ex, "Failed to initialize step {Name}", Name);
            return false;
        }
    }

    /// <summary>
    /// Called after the step is created or loaded.
    /// </summary>
    public async ValueTask<bool> TryAfterInitializedAsync()
    {
        try
        {
            if (StepBody is IAfterInitialized afterInitializeable)
            {
                await afterInitializeable.AfterInitializedAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning((int)EventLogType.Plugin, ex, "Failed after initialize step {Name}", Name);
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
