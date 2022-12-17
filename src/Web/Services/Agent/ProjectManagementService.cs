using Ayborg.Gateway.Agent.V1;
using AyBorg.Web.Services.AppState;
using Grpc.Core;
using Microsoft.AspNetCore.Components.Authorization;

namespace AyBorg.Web.Services.Agent;

public class ProjectManagementService : IProjectManagementService
{
    private readonly ILogger<ProjectManagementService> _logger;
    private readonly IStateService _stateService;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ProjectManagement.ProjectManagementClient _projectManagementClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectManagementService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    /// <param name="authenticationStateProvider">The authentication state provider.</param>
    /// <param name="projectManagementClient">The project management client.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    IStateService stateService,
                                    IAuthorizationHeaderUtilService authorizationHeaderUtilService,
                                    AuthenticationStateProvider authenticationStateProvider,
                                    ProjectManagement.ProjectManagementClient projectManagementClient)
    {
        _logger = logger;
        _stateService = stateService;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
        _authenticationStateProvider = authenticationStateProvider;
        _projectManagementClient = projectManagementClient;
    }

    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<IEnumerable<Shared.Models.Agent.ProjectMeta>> GetMetasAsync()
    {
        try
        {
            GetProjectMetasResponse response = await _projectManagementClient.GetProjectMetasAsync(new GetProjectMetasRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName
            });

            var result = new List<Shared.Models.Agent.ProjectMeta>();

            foreach (ProjectMeta? pm in response.ProjectMetas)
            {
                result.Add(new Shared.Models.Agent.ProjectMeta(pm));
            }

            return result;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to get project metas");
            return new List<Shared.Models.Agent.ProjectMeta>();
        }
    }

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<Shared.Models.Agent.ProjectMeta> GetActiveMetaAsync()
    {
        IEnumerable<Shared.Models.Agent.ProjectMeta> projectMetas = await GetMetasAsync();
        return projectMetas.FirstOrDefault(pm => pm.IsActive)!;
    }

    /// <summary>
    /// Creates asynchronous.
    /// </summary>
    /// <param name="projectName">Name of the project.</param>
    /// <returns></returns>
    /// <exception cref="System.Text.Json.JsonException"></exception>
    public async ValueTask<Shared.Models.Agent.ProjectMeta> CreateAsync(string projectName)
    {
        try
        {
            CreateProjectResponse response = await _projectManagementClient.CreateProjectAsync(new CreateProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectName = projectName
            });

            return new Shared.Models.Agent.ProjectMeta(response.ProjectMeta);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to create project");
            return null!;
        }
    }

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryDeleteAsync(Shared.Models.Agent.ProjectMeta projectMeta)
    {
        try
        {
            _ = await _projectManagementClient.DeleteProjectAsync(new DeleteProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectId = projectMeta.Id
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to delete project");
            return false;
        }
    }

    /// <summary>
    /// Activates the asynchronous.
    /// </summary>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryActivateAsync(Shared.Models.Agent.ProjectMeta projectMeta)
    {
        try
        {
            _ = await _projectManagementClient.ActivateProjectAsync(new ActivateProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectDbId = projectMeta.DbId
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to activate project");
            return false;
        }
    }

    /// <summary>
    /// Save project asynchronous.
    /// </summary>
    /// <param name="projectMeta">The project meta info.</param>
    /// <param name="projectSaveInfo">The project save information.</param>
    public async ValueTask<bool> TrySaveAsync(Shared.Models.Agent.ProjectMeta projectMeta,
                                                Shared.Models.Agent.ProjectSaveInfo projectSaveInfo)
    {
        try
        {
            _ = await _projectManagementClient.SaveProjectAsync(new SaveProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectDbId = projectMeta.DbId,
                ProjectId = projectMeta.Id,
                ProjectSaveInfo = await CreateRpcProjectSaveInfoAsync(projectSaveInfo)
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to save project");
            return false;
        }
    }

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="dbId">The database identifier.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryApproveAsync(string dbId,
                                                    Shared.Models.Agent.ProjectSaveInfo projectSaveInfo)
    {
        try
        {
            _ = await _projectManagementClient.ApproveProjectAsync(new ApproveProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectDbId = dbId,
                ProjectSaveInfo = await CreateRpcProjectSaveInfoAsync(projectSaveInfo)
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to approve project");
            return false;
        }
    }

    private async ValueTask<ProjectSaveInfo> CreateRpcProjectSaveInfoAsync(Shared.Models.Agent.ProjectSaveInfo projectSaveInfo)
    {
        AuthenticationState authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return new ProjectSaveInfo
        {
            State = (int)projectSaveInfo.State,
            VersionName = projectSaveInfo.VersionName,
            Comment = projectSaveInfo.Comment,
            UserName = authState.User.Identity!.Name
        };
    }
}
