using Autodroid.SDK.Data.DTOs;

namespace Autodroid.ServiceRegistry.Services;

public interface IKeeperService
{
    /// <summary>
    /// Finds service registry entries by name.
    /// </summary>
    /// <param name="name">The searched name.</param>
    /// <returns>Array of entries. Empty array if no entry match the name.</returns>
    Task<IEnumerable<ServiceRegistryEntryDto>> FindServiceRegistryEntriesAsync(string name);

    /// <summary>
    /// Gets all service registry entries.
    /// </summary>
    /// <returns>All service registry entries.</returns>
    Task<IEnumerable<ServiceRegistryEntryDto>> GetAllServiceRegistryEntriesAsync();

    /// <summary>
    /// Gets the service registry entry asynchronous.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    Task<ServiceRegistryEntryDto?> GetServiceRegistryEntryAsync(Guid serviceId);

    /// <summary>
    /// Register a new service.
    /// </summary>
    /// <param name="serviceRegistryEntry">Service registry entry.</param>
    /// <returns>Id for the new service.</returns>
    Task<Guid> RegisterAsync(ServiceRegistryEntryDto serviceRegistryEntry);

    /// <summary>
    /// Unregister a service
    /// </summary>
    /// <param name="serviceId">The service id.</param>
    Task UnregisterAsync(Guid serviceId);

    /// <summary>
    /// Updates the service timestamp.
    /// If not updated frequencly, the service will be recognized as not available and be removed to available service collection.
    /// </summary>
    /// <param name="serviceRegistryEntry">The desired service.</param>
    /// <returns>Task.</returns>
    Task UpdateTimestamp(ServiceRegistryEntryDto serviceRegistryEntry);
}