using Ayborg.Gateway.Agent.V1;

namespace AyBorg.Web.Services.Agent;

public class StorageService : IStorageService
{
    private readonly IStateService _stateService;
    private readonly Storage.StorageClient _storageClient;



    /// <summary>
    /// Initializes a new instance of the <see cref="StorageService"/> class.
    /// </summary>
    /// <param name="stateService">The state service.</param>
    /// <param name="storageClient">The storage client.</param>
    public StorageService(IStateService stateService, Storage.StorageClient storageClient)
    {
        _stateService = stateService;
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
