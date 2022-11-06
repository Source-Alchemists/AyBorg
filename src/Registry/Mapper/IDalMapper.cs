using Autodroid.SDK.Data.DAL;
using Autodroid.SDK.Data.DTOs;
using Autodroid.Registry.Models;

namespace Autodroid.Registry.Mapper;

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
    /// <param name="RegistryEntryDto">The service registry entry dto.</param>
    ServiceEntry Map(RegistryEntryDto serviceEntryDto);
}