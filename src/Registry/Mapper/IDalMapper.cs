using Atomy.SDK.Data.DAL;
using Atomy.SDK.Data.DTOs;
using Atomy.ServiceRegistry.Models;

namespace Atomy.ServiceRegistry.Mapper;

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