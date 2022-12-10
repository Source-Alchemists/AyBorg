using Ayborg.Gateway.V1;
using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services;

public class RegistryService : IRegistryService
{
    private readonly ILogger<RegistryService> _logger;
    private readonly Register.RegisterClient _registerClient;

    /// <summary>Initializes a new instance of the <see cref="RegistryService" /> class.</summary>
    /// <param name="logger">The logger.</param>
    /// <param name="gatewayConfiguration">The service configuration.</param>
    /// <param name="httpClient">The HTTP client.</param>
    public RegistryService(ILogger<RegistryService> logger, Register.RegisterClient registerClient)
    {
        _logger = logger;
        _registerClient = registerClient;
    }

    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <param name="serviceInfoEntry">The service registry entry.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    public string GetUrl(IEnumerable<ServiceInfoEntry> serviceInfoEntry, string serviceId)
    {
        ServiceInfoEntry? serviceDetails = serviceInfoEntry.FirstOrDefault(x => x.Id.Equals(serviceId, StringComparison.InvariantCultureIgnoreCase));
        if (serviceDetails == null)
        {
            _logger.LogWarning("Service with id {serviceId} not found", serviceId);
            return string.Empty;
        }
        return serviceDetails.Url;
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
