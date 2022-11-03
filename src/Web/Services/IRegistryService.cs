using Atomy.SDK.DTOs;

namespace Atomy.Web.Services;

public interface IRegistryService
{
    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <param name="serviceRegistryEntryDtos">The service registry entry dtos.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    string GetUrl(IEnumerable<ServiceRegistryEntryDto> serviceRegistryEntryDtos, string serviceId);

    /// <summary>Receives all available services asynchronous.</summary>
    /// <returns>
    ///  Service registry entries.
    ///  </returns>
    Task<IEnumerable<ServiceRegistryEntryDto>> ReceiveAllAvailableServicesAsync();

    /// <summary>
    /// Receives all available services asynchronous.
    /// </summary>
    /// <param name="typeName">The name.</param>
    /// <returns></returns>
    Task<IEnumerable<ServiceRegistryEntryDto>> ReceiveAllAvailableServicesAsync(string typeName);
}