using Ayborg.Gateway.V1;
using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services;

public class RegistryService : IRegistryService
{
    private readonly Register.RegisterClient _registerClient;

    /// <summary>Initializes a new instance of the <see cref="RegistryService" /> class.</summary>
    /// <param name="registerClient">The register client.</param>
    public RegistryService(Register.RegisterClient registerClient)
    {
        _registerClient = registerClient;
    }

    /// <summary>Receives all available services asynchronous.</summary>
    /// <returns>
    ///   Service registry entries.
    /// </returns>
    public async Task<IEnumerable<ServiceInfoEntry>> ReceiveServicesAsync(string typeName = "")
    {
        GetServicesResponse response = await _registerClient.GetServicesAsync(new GetServicesRequest
        {
            Id = string.Empty,
            Name = string.Empty,
            Type = typeName,
            UniqueName = string.Empty,
            Version = string.Empty
        });

        var result = new List<ServiceInfoEntry>();
        foreach (ServiceInfo? service in response.Services)
        {
            result.Add(new ServiceInfoEntry(service));
        }

        return result;
    }
}
