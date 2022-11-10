using Autodroid.SDK.Data.DTOs;
using Autodroid.SDK.System.Configuration;

namespace Autodroid.Web.Services;

public class RegistryService : IRegistryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegistryService> _logger;
    
    /// <summary>Initializes a new instance of the <see cref="RegistryService" /> class.</summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceConfiguration">The service configuration.</param>
    /// <param name="httpClient">The HTTP client.</param>
    public RegistryService(ILogger<RegistryService> logger, IServiceConfiguration serviceConfiguration, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(serviceConfiguration.RegistryUrl);
    }

    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <param name="RegistryEntryDtos">The service registry entry dtos.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    public string GetUrl(IEnumerable<RegistryEntryDto> RegistryEntryDtos, string serviceId)
    {
        var id = Guid.Parse(serviceId);
        var serviceDetails = RegistryEntryDtos.FirstOrDefault(x => x.Id.Equals(id));
        if (serviceDetails == null) return string.Empty;
        return serviceDetails.Url;
    }

    /// <summary>Receives all available services asynchronous.</summary>
    /// <returns>
    ///   Service registry entries.
    /// </returns>
    public async Task<IEnumerable<RegistryEntryDto>> ReceiveAllAvailableServicesAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<RegistryEntryDto[]>("/Services");
        if (result != null)
        {
            return result;
        } 

        _logger.LogWarning("Failed to receive service registry entries!");
        return new List<RegistryEntryDto>();
    }

    /// <summary>
    /// Receives all available services asynchronous.
    /// </summary>
    /// <param name="typeName">The name.</param>
    /// <returns></returns>
    public async Task<IEnumerable<RegistryEntryDto>> ReceiveAllAvailableServicesAsync(string typeName)
    {
        var result = await _httpClient.GetFromJsonAsync<RegistryEntryDto[]>($"/Services/type/{typeName}");
        if (result != null)
        {
            return result;
        } 

        _logger.LogWarning("Failed to receive service registry entries!");
        return new List<RegistryEntryDto>();
    }
}
