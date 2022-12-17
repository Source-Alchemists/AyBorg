using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Agent;
using AyBorg.SDK.System.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace AyBorg.Agent.Services;

internal sealed class CacheService : ICacheService
{
    private readonly ILogger<CacheService> _logger;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="mapper">The mapper.</param>
    /// <param name="memoryCache">The cache.</param>
    public CacheService(ILogger<CacheService> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _cache = memoryCache;
        _cacheEntryOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromSeconds(5),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15)
        };
    }

    /// <summary>
    /// Fills the cache with the values for the specified iteration.
    /// </summary>
    /// <param name="iteration">The iteration.</param>
    /// <param name="project">The project.</param>
    public void CreateCache(Guid iterationId, Project project)
    {
        _ = Parallel.ForEach(project.Steps, step =>
        {
            CreateStepEntry(iterationId, step);
            foreach (IPort port in step.Ports)
            {
                // Only input ports that are connected to another port are cached.
                // All other ports are not changing there displayed value at real time.
                if (port.Direction == PortDirection.Input && port.IsConnected)
                {
                    CreatePortEntry(iterationId, port);
                }
            }
        });
    }

    /// <summary>
    /// Gets or creates the port cache entry for the specified iteration.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    /// <remarks>If the iteration does not exist, it will create a port entry from the last iteration.</remarks>
    public Port GetOrCreatePortEntry(Guid iterationId, IPort port)
    {
        var key = new PortCacheKey(iterationId, port.Id);
        Port? result = _cache.GetOrCreate(key, entry =>
        {
            entry.SetOptions(_cacheEntryOptions);
            return RuntimeMapper.FromRuntime(port);
        });

        if (result == null)
        {
            _logger.LogWarning("No port entry found or created for iteration {IterationId} and port {PortId}.", iterationId, port.Id);
            throw new InvalidOperationException($"No port entry found or created for iteration {iterationId} and port {port.Id}.");
        }
        return result;
    }

    /// <summary>
    /// Gets or creates the step cache entry for the specified iteration.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="step">The step.</param>
    /// <returns></returns>
    /// <remarks>If the iteration does not exist, it will create a step entry from the last iteration.</remarks>
    public long GetOrCreateStepExecutionTimeEntry(Guid iterationId, IStepProxy stepProxy)
    {
        var key = new StepCacheKey(iterationId, stepProxy.Id);
        return _cache.GetOrCreate(key, entry =>
        {
            entry.SetOptions(_cacheEntryOptions);
            return stepProxy.ExecutionTimeMs;
        });
    }

    private void CreatePortEntry(Guid iterationId, IPort port)
    {
        var key = new PortCacheKey(iterationId, port.Id);
        Port portDto = RuntimeMapper.FromRuntime(port);
        _cache.Set(key, portDto, _cacheEntryOptions);
    }

    private void CreateStepEntry(Guid iterationId, IStepProxy stepProxy)
    {
        var key = new StepCacheKey(iterationId, stepProxy.Id);
        _cache.Set(key, stepProxy.ExecutionTimeMs, _cacheEntryOptions);
    }
}
