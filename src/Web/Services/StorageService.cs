using System.Net;

namespace Autodroid.Web.Services;

public class StorageService : IStorageService
{
    private readonly ILogger<StorageService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public StorageService(ILogger<StorageService> logger,
                            HttpClient httpClient,
                            IAuthorizationHeaderUtilService authorizationHeaderUtilService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
    }

    /// <summary>
    /// Gets the directories.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> GetDirectoriesAsync(string baseUrl, string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/storage/directories?path={path}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return Enumerable.Empty<string>();
        }
        var directories = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
        return directories!;
    }
}