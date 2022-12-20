using Ayborg.Gateway.Agent.V1;
using AyBorg.Web.Services.AppState;

namespace AyBorg.Web.Services.Agent;

public class StorageService : IStorageService
{
    private readonly IStateService _stateService;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly Storage.StorageClient _storageClient;



    /// <summary>
    /// Initializes a new instance of the <see cref="StorageService"/> class.
    /// </summary>
    /// <param name="stateService">The state service.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    /// <param name="storageClient">The storage client.</param>
    public StorageService(IStateService stateService, IAuthorizationHeaderUtilService authorizationHeaderUtilService, Storage.StorageClient storageClient)
    {
        _stateService = stateService;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
        _storageClient = storageClient;
    }

    /// <summary>
    /// Gets the directories.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> GetDirectoriesAsync(string path)
    {
        GetDirectoriesResponse response = await _storageClient.GetDirectoriesAsync(new GetDirectoriesRequest
        {
            AgentUniqueName = _stateService.AgentState.UniqueName,
            Path = path
        });

        return response.Directories;
    }
}