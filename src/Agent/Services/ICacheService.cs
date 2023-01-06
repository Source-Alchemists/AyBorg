using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

public interface ICacheService
{
    /// <summary>
    /// Gets the size of the cache.
    /// </summary>
    int CacheSize { get; }

    /// <summary>
    /// Fills the cache with the values for the specified iteration.
    /// </summary>
    /// <param name="iteration">The iteration.</param>
    /// <param name="project">The project.</param>
    void CreateCache(Guid iterationId, Project project);

    /// <summary>
    /// Gets or creates the step cache entry for the specified iteration.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="step">The step.</param>
    /// <returns></returns>
    /// <remarks>If the iteration does not exist, it will create a step entry from the last iteration.</remarks>
    Step GetOrCreateStepEntry(Guid iterationId, IStepProxy step);

    /// <summary>
    /// Gets or creates the port cache entry for the specified iteration.
    /// </summary>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    /// <remarks>If the iteration does not exist, it will create a port entry from the last iteration.</remarks>
    Port GetOrCreatePortEntry(Guid iterationId, IPort port);
}
