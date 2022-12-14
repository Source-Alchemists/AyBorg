using AyBorg.SDK.Data.DTOs;

namespace AyBorg.Registry.Services;

public interface IKeeperService
{
    /// <summary>
    /// Finds service registry entries by name.
    /// </summary>
    /// <param name="name">The searched name.</param>
    /// <returns>Array of entries. Empty array if no entry match the name.</returns>
    ValueTask<IEnumerable<RegistryEntryDto>> FindRegistryEntriesAsync(string name);

    /// <summary>
    /// Gets all service registry entries.
    /// </summary>
    /// <returns>All service registry entries.</returns>
    ValueTask<IEnumerable<RegistryEntryDto>> GetAllRegistryEntriesAsync();

    /// <summary>
    /// Gets the service registry entry asynchronous.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    ValueTask<RegistryEntryDto?> GetRegistryEntryAsync(Guid serviceId);

    /// <summary>
    /// Register a new service.
    /// </summary>
    /// <param name="RegistryEntry">Service registry entry.</param>
    /// <returns>Id for the new service.</returns>
    Task<Guid> RegisterAsync(RegistryEntryDto RegistryEntry);

    /// <summary>
    /// Unregister a service
    /// </summary>
    /// <param name="serviceId">The service id.</param>
    Task UnregisterAsync(Guid serviceId);

    /// <summary>
    /// Updates the service timestamp.
    /// If not updated frequencly, the service will be recognized as not available and be removed to available service collection.
    /// </summary>
    /// <param name="RegistryEntry">The desired service.</param>
    /// <returns>ValueTask.</returns>
    ValueTask UpdateTimestamp(RegistryEntryDto RegistryEntry);
}