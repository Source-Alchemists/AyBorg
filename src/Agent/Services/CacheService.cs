using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Agent;
using AyBorg.SDK.System.Caching;

namespace AyBorg.Agent.Services;

internal sealed class CacheService : ICacheService
{
    private readonly ILogger<CacheService> _logger;
    private readonly MemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="mapper">The mapper.</param>
    public CacheService(ILogger<CacheService> logger)
    {
        _logger = logger;
        _memoryCache = MemoryCache.Default;
    }

    /// <summary>
    /// Fills the cache with the values for the specified iteration.
    /// </summary>
    /// <param name="iteration">The iteration.</param>
    /// <param name="project">The project.</param>
    public async ValueTask CreateCacheAsync(Guid iterationId, Project project)
    {
        CancellationToken token = CancellationToken.None;
        await Parallel.ForEachAsync(project.Steps, async (step, token) =>
        {
            await GetOrCreateStepEntryAsync(iterationId, step);
        });
    }

    /// <summary>
    /// Gets or creates the step cache entry for the specified iteration.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="step">The step.</param>
    /// <returns></returns>
    /// <remarks>If the iteration does not exist, it will create a step entry from the last iteration.</remarks>
    public async ValueTask<Step> GetOrCreateStepEntryAsync(Guid iterationId, IStepProxy step)
    {
        Step cachedStep = await GetOrCreateStepMetaEntryAsync(iterationId, step);
        var cachedPorts = new HashSet<Port>();
        foreach (IPort port in step.Ports)
        {
            // Only input ports that are connected to another port are cached.
            // All other ports are not changing there displayed value at real time.
            if (port.Direction == PortDirection.Input && port.IsConnected)
            {
                Port cachedPort = await GetOrCreatePortEntryAsync(iterationId, port);
                cachedPorts.Add(cachedPort);
            }
        }

        cachedStep.Ports = cachedPorts;
        return cachedStep;
    }

    /// <summary>
    /// Gets or creates the port cache entry for the specified iteration.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    /// <remarks>If the iteration does not exist, it will create a port entry from the last iteration.</remarks>
    public async ValueTask<Port> GetOrCreatePortEntryAsync(Guid iterationId, IPort port)
    {
        var key = new PortCacheKey(iterationId, port.Id);
        string keyStr = key.ToString();
        CacheItem cacheItem = _memoryCache.GetCacheItem(keyStr);
        Port? result;
        if (cacheItem == null)
        {
            result = await RuntimeMapper.FromRuntimeAsync(port);
            _memoryCache.Add(new CacheItem(keyStr, result), CreateCacheItemPolicy());
        }
        else
        {
            result = (Port)cacheItem.Value;
        }

        if (result == null)
        {
            _logger.LogWarning("No port entry found or created for iteration {IterationId} and port {PortId}.", iterationId, port.Id);
            throw new InvalidOperationException($"No port entry found or created for iteration {iterationId} and port {port.Id}.");
        }
        return result;
    }

    private async ValueTask<Step> GetOrCreateStepMetaEntryAsync(Guid iterationId, IStepProxy stepProxy)
    {
        var key = new StepCacheKey { IterationId = iterationId, StepId = stepProxy.Id };
        string keyStr = key.ToString();
        CacheItem cacheItem = _memoryCache.GetCacheItem(keyStr);
        if (cacheItem == null)
        {
            Step step = await RuntimeMapper.FromRuntimeAsync(stepProxy, true);
            _memoryCache.Add(new CacheItem(keyStr, step), CreateCacheItemPolicy());
            return step;
        }
        else
        {
            return (Step)cacheItem.Value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CacheItemPolicy CreateCacheItemPolicy()
    {
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5),
            RemovedCallback = CacheItemRemovedCallback
        };

        return policy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CacheItemRemovedCallback(CacheEntryRemovedArguments arguments)
    {
        if (arguments.CacheItem.Value is Port oldPort)
        {
            oldPort.Dispose();
        }
    }
}
