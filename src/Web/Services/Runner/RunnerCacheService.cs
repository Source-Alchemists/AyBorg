using Autodroid.SDK.System.Caching;
using Autodroid.SDK.Data.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Autodroid.Web.Services.Agent;

public class AgentCacheService : IAgentCacheService
{
    private readonly ILogger<AgentCacheService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentCacheService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="memoryCache">The cache.</param>
    /// <param name="httpClient">The HTTP client.</param>
    public AgentCacheService(ILogger<AgentCacheService> logger, IMemoryCache memoryCache, HttpClient httpClient)
    {
        _logger = logger;
        _cache = memoryCache;
        _httpClient = httpClient;
        _cacheEntryOptions = new MemoryCacheEntryOptions {
            SlidingExpiration = TimeSpan.FromMinutes(2),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        };
    }

    /// <summary>
    /// Gets or creates the cache entry for the specified iteration.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    public async Task<PortDto> GetOrCreatePortEntryAsync(string baseUrl, Guid portId, Guid iterationId)
    {
        var key = new PortCacheKey(iterationId, portId);
        return await _cache.GetOrCreateAsync<PortDto>(key, async entry => 
        {
            entry.SetOptions(_cacheEntryOptions);
            var response = await _httpClient.GetFromJsonAsync<PortDto>($"{baseUrl}/flow/ports/{portId}/{iterationId}");
            if(response == null)
            {
                _logger.LogWarning("Could not get port {portId} for iteration {iterationId} from {baseUrl}", portId, iterationId, baseUrl);
                throw new Exception($"Could not get port {portId} for iteration {iterationId} from {baseUrl}");
            }
            return response;
        });
        
    }
}