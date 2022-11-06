using Autodroid.SDK.Data.DTOs;

namespace Autodroid.Web.Services;

public interface IRegistryService
{
    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <param name="RegistryEntryDtos">The service registry entry dtos.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    string GetUrl(IEnumerable<RegistryEntryDto> RegistryEntryDtos, string serviceId);

    /// <summary>Receives all available services asynchronous.</summary>
    /// <returns>
    ///  Service registry entries.
    ///  </returns>
    Task<IEnumerable<RegistryEntryDto>> ReceiveAllAvailableServicesAsync();

    /// <summary>
    /// Receives all available services asynchronous.
    /// </summary>
    /// <param name="typeName">The name.</param>
    /// <returns></returns>
    Task<IEnumerable<RegistryEntryDto>> ReceiveAllAvailableServicesAsync(string typeName);
}