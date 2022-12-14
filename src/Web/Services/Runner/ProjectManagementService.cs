using System.Text;
using System.Text.Json;
using AyBorg.SDK.Data.DTOs;

namespace AyBorg.Web.Services.Agent;

public class ProjectManagementService : IProjectManagementService
{
    private readonly ILogger<ProjectManagementService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectManagementService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    HttpClient httpClient,
                                    IAuthorizationHeaderUtilService authorizationHeaderUtilService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
    }

    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns></returns>
    public async Task<IEnumerable<ProjectMetaDto>> GetMetasAsync(string baseUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/projects");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        IEnumerable<ProjectMetaDto>? result = await response.Content.ReadFromJsonAsync<IEnumerable<ProjectMetaDto>>();
        if (result != null)
        {
            return result;
        }

        return new List<ProjectMetaDto>();
    }

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns></returns>
    public async Task<ProjectMetaDto> GetActiveMetaAsync(string baseUrl)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/projects/active");
            request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
            HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            ProjectMetaDto? result = await response.Content.ReadFromJsonAsync<ProjectMetaDto>();
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "Failed to get active project meta");
            return null!;
        }
    }

    /// <summary>
    /// Creates asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <returns></returns>
    /// <exception cref="System.Text.Json.JsonException"></exception>
    public async Task<ProjectMetaDto> CreateAsync(string baseUrl, string projectName)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/projects?name={projectName}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not create project '{projectName}' [Code: {response.StatusCode}]!", projectName, response.StatusCode);
            return null!;
        }

        using Stream stream = response.Content.ReadAsStream();
        var streamReader = new StreamReader(stream, Encoding.UTF8);
        string postJson = await streamReader.ReadToEndAsync();

        ProjectMetaDto? projectMetaDto = JsonSerializer.Deserialize<ProjectMetaDto>(postJson);
        if (projectMetaDto == null) throw new JsonException();
        return projectMetaDto;
    }

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    public async Task<bool> TryDeleteAsync(string baseUrl, ProjectMetaDto projectMeta)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/projects?id={projectMeta.Id}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not delete project '{projectMeta.Name}' [Code: {response.StatusCode}]!", projectMeta.Name, response.StatusCode);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Activates the asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    public async Task<bool> TryActivateAsync(string baseUrl, ProjectMetaDto projectMeta)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/projects/{projectMeta.DbId}/active/true");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not set project '{projectMeta.Name}' to active [Code: {response.StatusCode}]!", projectMeta.Name, response.StatusCode);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Save project as ready asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="dbId">The database identifier.</param>
    /// <param name="projectStateChange">State of the project.</param>
    /// <returns></returns>
    public async Task<bool> TrySaveNewVersionAsync(string baseUrl, Guid dbId, ProjectStateChangeDto projectStateChange)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/projects/{dbId}/state")
        {
            Content = new StringContent(JsonSerializer.Serialize(projectStateChange), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not save project '{dbId}' as new version [Code: {response.StatusCode}]!", dbId, response.StatusCode);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="dbId">The database identifier.</param>
    /// <param name="projectStateChange">State of the project.</param>
    /// <returns></returns>
    public async Task<bool> TryApproveAsnyc(string baseUrl, Guid dbId, ProjectStateChangeDto projectStateChange)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/projects/{dbId}/approve")
        {
            Content = new StringContent(JsonSerializer.Serialize(projectStateChange), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not approve project '{dbId}' [Code: {response.StatusCode}]!", dbId, response.StatusCode);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Save project asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta info.</param>
    public async Task<bool> TrySaveAsync(string baseUrl, ProjectMetaDto projectMeta)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/projects/{projectMeta.Id}");
        request.Headers.Authorization = await _authorizationHeaderUtilService.GenerateAsync();
        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not save project '{projectMeta.Name}' [Code: {response.StatusCode}]!", projectMeta.Name, response.StatusCode);
            return false;
        }

        return true;
    }
}
