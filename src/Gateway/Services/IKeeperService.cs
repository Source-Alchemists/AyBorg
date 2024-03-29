using AyBorg.Gateway.Models;

namespace AyBorg.Gateway.Services;

public interface IKeeperService
{
    /// <summary>
    /// Finds service registry entries by name.
    /// </summary>
    /// <param name="name">The searched name.</param>
    /// <returns>Array of entries. Empty array if no entry match the name.</returns>
    ValueTask<IEnumerable<ServiceEntry>> FindRegistryEntriesAsync(string name);

    /// <summary>
    /// Gets all service registry entries.
    /// </summary>
    /// <returns>All service registry entries.</returns>
    ValueTask<IEnumerable<ServiceEntry>> GetAllRegistryEntriesAsync();

    /// <summary>
    /// Gets the service registry entry asynchronous.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    ValueTask<ServiceEntry?> GetRegistryEntryAsync(Guid serviceId);

    /// <summary>
    /// Register a new service.
    /// </summary>
    /// <param name="serviceEntry">Service registry entry.</param>
    /// <returns>Id for the new service.</returns>
    Task<Guid> RegisterAsync(ServiceEntry serviceEntry);

    /// <summary>
    /// Unregister a service
    /// </summary>
    /// <param name="serviceId">The service id.</param>
    /// <returns>ServiceEntry if the service was found and removed. Null if the service was not found.</returns>
    ServiceEntry? Unregister(Guid serviceId);

    /// <summary>
    /// Updates the service timestamp.
    /// If not updated frequencly, the service will be recognized as not available and be removed to available service collection.
    /// </summary>
    /// <param name="serviceEntry">The desired service.</param>
    /// <returns>ValueTask.</returns>
    ValueTask UpdateTimestamp(ServiceEntry serviceEntry);
}
