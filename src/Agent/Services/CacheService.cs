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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AyBorg.Runtime;
using AyBorg.Runtime.Projects;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;

namespace AyBorg.Agent.Services;

internal sealed class CacheService : ICacheService
{
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
    public CacheService(IRuntimeMapper runtimeMapper, IConfiguration configuration)
    {
        _runtimeMapper = runtimeMapper;
        _maxCacheTimeSeconds = configuration.GetValue("AyBorg:Cache:MaxSeconds", 10);
        _maxCacheIterations = configuration.GetValue("AyBorg:Cache:MaxIterations", 5);
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
            try
            {
                CreateStepEntry(iterationId, step);
            }
            catch
            {
                // Non critical exception, may the step is already creating new outputs.
            }
        });
    }

    /// <summary>
    /// Gets or creates the step cache entry for the specified iteration.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="step">The step.</param>
    /// <returns></returns>
    /// <remarks>If the iteration does not exist, it will create a step entry from the last iteration.</remarks>
    public StepModel GetOrCreateStepEntry(Guid iterationId, IStepProxy step)
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
    public PortModel GetOrCreatePortEntry(Guid iterationId, IPort port)
    {
        object cacheItem = GetCacheItem(_cache, iterationId, port.Id);
        cacheItem ??= CreatePortEntry(iterationId, port);
        return (PortModel)cacheItem;
    }

    private StepModel GetOrCreateStepMetaEntry(Guid iterationId, IStepProxy stepProxy)
    {
        object cacheItem = GetCacheItem(_cache, iterationId, stepProxy.Id);
        cacheItem ??= CreateStepEntry(iterationId, stepProxy);
        return (StepModel)cacheItem;
    }

    private StepModel CreateStepEntry(Guid iterationId, IStepProxy step)
    {
        StepModel cachedStep = CreateStepMetaEntry(iterationId, step);
        var cachedPorts = new HashSet<PortModel>();
        foreach (IPort port in step.Ports)
        {
            PortModel cachedPort = CreatePortEntry(iterationId, port);
            cachedPorts.Add(cachedPort);
        }

        cachedStep.Ports = cachedPorts;
        return cachedStep;
    }

    private StepModel CreateStepMetaEntry(Guid iterationId, IStepProxy stepProxy)
    {
        StepModel step = _runtimeMapper.FromRuntime(stepProxy, true);
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

    private PortModel CreatePortEntry(Guid iterationId, IPort port)
    {
        PortModel? result;
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
