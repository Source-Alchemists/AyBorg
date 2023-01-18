using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services;

public interface IRegistryService
{
    /// <summary>Receives all available services asynchronous.</summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>
    ///  Service registry entries.
    ///  </returns>
    Task<IEnumerable<ServiceInfoEntry>> ReceiveServicesAsync(string typeName = "");
}
