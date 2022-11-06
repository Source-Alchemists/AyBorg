using Autodroid.SDK.Data.DAL;
using Autodroid.SDK.Data.DTOs;
using Autodroid.ServiceRegistry.Models;

namespace Autodroid.ServiceRegistry.Mapper;

public interface IDalMapper 
{
    /// <summary>
    /// Maps the specified service entry.
    /// </summary>
    /// <param name="serviceEntry">The service entry.</param>
    ServiceEntryRecord Map(ServiceEntry serviceEntry);

    /// <summary>
    /// Maps the specified service entry record.
    /// </summary>
    /// <param name="serviceEntryRecord">The service entry record.</param>
    ServiceEntry Map(ServiceEntryRecord serviceEntryRecord);

    /// <summary>
    /// Maps the specified service registry entry dto.
    /// </summary>
    /// <param name="serviceRegistryEntryDto">The service registry entry dto.</param>
    ServiceEntry Map(ServiceRegistryEntryDto serviceEntryDto);
}