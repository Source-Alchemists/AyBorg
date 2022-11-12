using AyBorg.SDK.Data.DTOs;

namespace AyBorg.Web.Services.Agent;

public interface IAgentCacheService
{
    /// <summary>
    /// Gets or creates the cache entry for the specified iteration.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    Task<PortDto> GetOrCreatePortEntryAsync(string baseUrl, Guid portId, Guid iterationId);
}