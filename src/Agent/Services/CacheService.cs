using Microsoft.Extensions.Caching.Memory;
using Atomy.SDK.Data.DTOs;
using Atomy.SDK.System.Mapper;
using Atomy.SDK.Common.Ports;
using Atomy.SDK.Projects;
using Atomy.SDK.System.Caching;
using Atomy.SDK.Common;

namespace Atomy.Agent.Services;

public class CacheService : ICacheService
{
    private readonly ILogger<CacheService> _logger;
    private readonly IDtoMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="mapper">The mapper.</param>
    /// <param name="memoryCache">The cache.</param>
    public CacheService(ILogger<CacheService> logger, IDtoMapper mapper, IMemoryCache memoryCache)
    {
        _logger = logger;
        _mapper = mapper;
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
        Parallel.ForEach(project.Steps, step =>
        {
            CreateStepEntry(iterationId, step);
            foreach (var port in step.Ports)
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
    public PortDto GetOrCreatePortEntry(Guid iterationId, IPort port)
    {
        var key = new PortCacheKey(iterationId, port.Id);
        return _cache.GetOrCreate<PortDto>(key, entry =>
        {
            entry.SetOptions(_cacheEntryOptions);
            return _mapper.Map(port);
        });
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
        return _cache.GetOrCreate<long>(key, entry =>
        {
            entry.SetOptions(_cacheEntryOptions);
            return stepProxy.ExecutionTimeMs;
        });
    }

    private void CreatePortEntry(Guid iterationId, IPort port)
    {
        var key = new PortCacheKey(iterationId, port.Id);
        var portDto = _mapper.Map(port);
        _cache.Set<PortDto>(key, portDto, _cacheEntryOptions);
    }

    private void CreateStepEntry(Guid iterationId, IStepProxy stepProxy)
    {
        var key = new StepCacheKey(iterationId, stepProxy.Id);
        _cache.Set<long>(key, stepProxy.ExecutionTimeMs, _cacheEntryOptions);
    }
}