using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Agent;

namespace AyBorg.Agent.Services;

internal sealed class CacheService : ICacheService
{
    private readonly ILogger<CacheService> _logger;
    private readonly IRuntimeMapper _runtimeMapper;
    private readonly ConcurrentDictionary<CacheKey, ConcurrentBag<CacheItem>> _cache = new();
    private readonly int _maxCacheTimeSeconds;
    private readonly int _maxCacheIterations;

    /// <summary>
    /// Gets the size of the cache.
    /// </summary>
    public int CacheSize => _cache.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public CacheService(ILogger<CacheService> logger, IRuntimeMapper runtimeMapper, IConfiguration configuration)
    {
        _logger = logger;
        _runtimeMapper = runtimeMapper;
        _maxCacheTimeSeconds = configuration.GetValue("AyBorg:Cache:MaxSeconds", 10);
        _maxCacheIterations = configuration.GetValue("AyBorg:Cache:MaxIterations", 5);

        _logger.LogTrace("CacheService initialized with {maxCacheTimeSeconds} seconds and {maxCacheIterations} iterations.", _maxCacheTimeSeconds, _maxCacheIterations);
    }

    /// <summary>
    /// Fills the cache with the values for the specified iteration.
    /// </summary>
    /// <param name="iteration">The iteration.</param>
    /// <param name="project">The project.</param>
    public void CreateCache(Guid iterationId, Project project)
    {
        ClearOutdatedCache(_cache);
        _cache.TryAdd(new CacheKey(iterationId), new ConcurrentBag<CacheItem>());
        Parallel.ForEach(project.Steps, (step) =>
        {
            CreateStepEntry(iterationId, step);
        });
    }

    /// <summary>
    /// Gets or creates the step cache entry for the specified iteration.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="step">The step.</param>
    /// <returns></returns>
    /// <remarks>If the iteration does not exist, it will create a step entry from the last iteration.</remarks>
    public Step GetOrCreateStepEntry(Guid iterationId, IStepProxy step)
    {
        return GetOrCreateStepMetaEntry(iterationId, step);
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
        object cacheItem = GetCacheItem(_cache, iterationId, port.Id);
        cacheItem ??= CreatePortEntry(iterationId, port);
        return (Port)cacheItem;
    }

    private Step GetOrCreateStepMetaEntry(Guid iterationId, IStepProxy stepProxy)
    {
        object cacheItem = GetCacheItem(_cache, iterationId, stepProxy.Id);
        cacheItem ??= CreateStepEntry(iterationId, stepProxy);
        return (Step)cacheItem;
    }

    private Step CreateStepEntry(Guid iterationId, IStepProxy step)
    {
        Step cachedStep = CreateStepMetaEntry(iterationId, step);
        var cachedPorts = new HashSet<Port>();
        foreach (IPort port in step.Ports)
        {
            // Only input ports that are connected to another port are cached.
            // All other ports are not changing there displayed value at real time.
            if (port.Direction == PortDirection.Input && port.IsConnected)
            {
                Port cachedPort = CreatePortEntry(iterationId, port);
                cachedPorts.Add(cachedPort);
            }
        }

        cachedStep.Ports = cachedPorts;
        return cachedStep;
    }

    private Step CreateStepMetaEntry(Guid iterationId, IStepProxy stepProxy)
    {
        Step step = _runtimeMapper.FromRuntime(stepProxy, true);
        var cacheItem = new CacheItem(stepProxy.Id, step);
        CacheKey? key = _cache.Keys.FirstOrDefault(k => k.Id.Equals(iterationId));
        if (key != null && _cache.TryGetValue(key, out ConcurrentBag<CacheItem>? value))
        {
            if (!value.Any(v => v.Id.Equals(stepProxy.Id)))
            {
                value.Add(cacheItem);
            }
        }
        else
        {
            _cache.TryAdd(new CacheKey(iterationId), new ConcurrentBag<CacheItem> { cacheItem });
        }
        return step;
    }

    private Port CreatePortEntry(Guid iterationId, IPort port)
    {
        Port? result;
        result = _runtimeMapper.FromRuntime(port);
        var cacheItem = new CacheItem(port.Id, result);
        CacheKey? key = _cache.Keys.FirstOrDefault(k => k.Id.Equals(iterationId));
        if (key != null
            && _cache.TryGetValue(key, out ConcurrentBag<CacheItem>? value)
            && !value.Any(v => v.Id.Equals(port.Id)))
        {
            value.Add(cacheItem);
        }

        _cache.TryAdd(new CacheKey(iterationId), new ConcurrentBag<CacheItem> { cacheItem });
        return result;
    }

    private void ClearOutdatedCache(ConcurrentDictionary<CacheKey, ConcurrentBag<CacheItem>> cache)
    {
        IOrderedEnumerable<CacheKey> cacheKeys = cache.Keys.GroupBy(k => k.Id).Select(g => g.First()).OrderBy(k => k.CreateTime);
        int cacheSize = cacheKeys.Count();
        IEnumerable<CacheKey> tmpOutdatedKeys;
        if (cacheKeys.Count() > _maxCacheIterations)
        {
            tmpOutdatedKeys = cacheKeys.TakeLast(cacheSize + 1 - _maxCacheIterations);
        }
        else
        {
            tmpOutdatedKeys = cacheKeys.Where(k => k.CreateTime + TimeSpan.FromSeconds(_maxCacheTimeSeconds) < DateTime.UtcNow);
        }

        List<CacheKey> outdatedKeys = new();
        foreach (CacheKey tk in tmpOutdatedKeys)
        {
            outdatedKeys.AddRange(cache.Keys.Where(k => k.Id.Equals(tk.Id)));
        }

        foreach (CacheKey outdatedKey in outdatedKeys)
        {
            cache.Remove(outdatedKey, out ConcurrentBag<CacheItem>? items);
            foreach (CacheItem item in items!)
            {
                if (item.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            items.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static KeyValuePair<CacheKey, ConcurrentBag<CacheItem>> GetCachePairFromLastIteration(ConcurrentDictionary<CacheKey, ConcurrentBag<CacheItem>> cache, Guid objectId)
    {
        return cache.Where(c => c.Value.Any(v => v.Id.Equals(objectId))).OrderBy(c => c.Key.CreateTime).LastOrDefault();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object GetCacheItem(ConcurrentDictionary<CacheKey, ConcurrentBag<CacheItem>> cache, Guid iterationId, Guid objectId)
    {
        CacheKey? key = cache.Keys.FirstOrDefault(k => k.Id.Equals(iterationId));
        if (key == null)
        {
            key = GetCachePairFromLastIteration(cache, objectId).Key;
            if (key == null)
            {
                return null!;
            }
        }

        ConcurrentBag<CacheItem> iterationObjects = cache[key];
        CacheItem? cacheItem = iterationObjects.FirstOrDefault(o => o.Id.Equals(objectId));
        if (cacheItem == null)
        {
            KeyValuePair<CacheKey, ConcurrentBag<CacheItem>> pair = GetCachePairFromLastIteration(cache, objectId);
            if (pair.Value != null)
            {
                return pair.Value.First(o => o.Id.Equals(objectId)).Value;
            }

            return null!;
        }

        return cacheItem.Value;
    }

    private sealed record CacheKey
    {
        public Guid Id { get; }

        public DateTime CreateTime { get; } = DateTime.UtcNow;

        public CacheKey(Guid id)
        {
            Id = id;
        }
    }

    private sealed record CacheItem
    {
        public Guid Id { get; } = Guid.Empty;

        public object Value { get; } = null!;

        public CacheItem(Guid id, object value)
        {
            Id = id;
            Value = value;
        }
    }
}
