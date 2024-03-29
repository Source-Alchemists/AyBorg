using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common;
using Grpc.Core;
using Microsoft.AspNetCore.Components.Authorization;

namespace AyBorg.Web.Services.Agent;

public class ProjectManagementService : IProjectManagementService
{
    private readonly ILogger<ProjectManagementService> _logger;
    private readonly IStateService _stateService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ProjectManagement.ProjectManagementClient _projectManagementClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectManagementService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="authenticationStateProvider">The authentication state provider.</param>
    /// <param name="projectManagementClient">The project management client.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    IStateService stateService,
                                    AuthenticationStateProvider authenticationStateProvider,
                                    ProjectManagement.ProjectManagementClient projectManagementClient)
    {
        _logger = logger;
        _stateService = stateService;
        _authenticationStateProvider = authenticationStateProvider;
        _projectManagementClient = projectManagementClient;
    }

    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <returns></returns>
    public ValueTask<IEnumerable<Shared.Models.Agent.ProjectMeta>> GetMetasAsync() => GetMetasAsync(_stateService.AgentState.UniqueName);

    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns></returns>
    public async ValueTask<IEnumerable<Shared.Models.Agent.ProjectMeta>> GetMetasAsync(string serviceUniqueName)
    {
        try
        {
            GetProjectMetasResponse response = await _projectManagementClient.GetProjectMetasAsync(new GetProjectMetasRequest
            {
                AgentUniqueName = serviceUniqueName
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
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to get project metas!");
            return new List<Shared.Models.Agent.ProjectMeta>();
        }
    }

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <returns></returns>
    public ValueTask<Shared.Models.Agent.ProjectMeta> GetActiveMetaAsync() => GetActiveMetaAsync(_stateService.AgentState.UniqueName);

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns></returns>
    public async ValueTask<Shared.Models.Agent.ProjectMeta> GetActiveMetaAsync(string serviceUniqueName)
    {
        IEnumerable<Shared.Models.Agent.ProjectMeta> projectMetas = await GetMetasAsync(serviceUniqueName);
        return projectMetas.FirstOrDefault(pm => pm.IsActive) ?? new Shared.Models.Agent.ProjectMeta();
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
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Creating project [{projectName}]", projectName);
            CreateProjectResponse response = await _projectManagementClient.CreateProjectAsync(new CreateProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectName = projectName
            });

            return new Shared.Models.Agent.ProjectMeta(response.ProjectMeta);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to create project!");
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
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Deleting project [{projectName}]", projectMeta.Name);
            _ = await _projectManagementClient.DeleteProjectAsync(new DeleteProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectId = projectMeta.Id
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to delete project!");
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
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Activating project [{projectName}]", projectMeta.Name);
            _ = await _projectManagementClient.ActivateProjectAsync(new ActivateProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectDbId = projectMeta.DbId
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to activate project!");
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
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Saving project [{projectName}] as [{projectState}].", projectMeta.Name, projectSaveInfo.State);
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
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to save project!");
            return false;
        }
    }

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="projectMeta">The project meta info.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryApproveAsync(Shared.Models.Agent.ProjectMeta projectMeta,
                                                    Shared.Models.Agent.ProjectSaveInfo projectSaveInfo)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Approving project [{projectName}] version [{projectVersion}].", projectMeta.Name, projectMeta.VersionName);
            _ = await _projectManagementClient.ApproveProjectAsync(new ApproveProjectRequest
            {
                AgentUniqueName = _stateService.AgentState.UniqueName,
                ProjectDbId = projectMeta.DbId,
                ProjectSaveInfo = await CreateRpcProjectSaveInfoAsync(projectSaveInfo)
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to approve project!");
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
