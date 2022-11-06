using Autodroid.SDK.Data.DTOs;

namespace Autodroid.Web.Services.Agent;

public class PluginsService
{
    private readonly ILogger<PluginsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public PluginsService(ILogger<PluginsService> logger, 
                            HttpClient httpClient, 
                            IAuthorizationHeaderUtilService authorizationHeaderUtilService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
    }

    /// <summary>
    /// Receive steps from the Agent, using a web service.
    /// </summary>
    public async Task<IEnumerable<StepDto>> ReceiveStepsAsync(string baseUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/plugins/steps");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        var steps = await response.Content.ReadFromJsonAsync<IEnumerable<StepDto>>();
        if (steps == null)
        {
            _logger.LogWarning("No steps received from Agent.");
            return new List<StepDto>();
        }
        return steps;
    }
}
