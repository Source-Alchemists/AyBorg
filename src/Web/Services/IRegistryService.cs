using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services;

public interface IRegistryService
{
    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <param name="serviceInfoEntry">The service registry entry dtos.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    string GetUrl(IEnumerable<ServiceInfoEntry> serviceInfoEntry, string serviceId);

    /// <summary>Receives all available services asynchronous.</summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>
    ///  Service registry entries.
    ///  </returns>
    Task<IEnumerable<ServiceInfoEntry>> ReceiveServicesAsync(string typeName = "");
}
