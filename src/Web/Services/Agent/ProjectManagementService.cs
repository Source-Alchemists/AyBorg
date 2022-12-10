using Ayborg.Gateway.V1;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public class ProjectManagementService : IProjectManagementService
{
    private readonly ILogger<ProjectManagementService> _logger;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly AgentProjectManagement.AgentProjectManagementClient _agentProjectManagementClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectManagementService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    IAuthorizationHeaderUtilService authorizationHeaderUtilService,
                                    AgentProjectManagement.AgentProjectManagementClient agentProjectManagementClient)
    {
        _logger = logger;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
        _agentProjectManagementClient = agentProjectManagementClient;
    }

    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <param name="baseUrl">The agent unique name.</param>
    /// <returns></returns>
    public async ValueTask<IEnumerable<Shared.Models.Agent.ProjectMeta>> GetMetasAsync(string agentUniqueName)
    {
        try
        {
            GetProjectMetasResponse response = await _agentProjectManagementClient.GetProjectMetasAsync(new GetProjectMetasRequest
            {
                AgentUniqueName = agentUniqueName
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
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <returns></returns>
    public async ValueTask<Shared.Models.Agent.ProjectMeta> GetActiveMetaAsync(string agentUniqueName)
    {
        IEnumerable<Shared.Models.Agent.ProjectMeta> projectMetas = await GetMetasAsync(agentUniqueName);
        return projectMetas.FirstOrDefault(pm => pm.IsActive)!;
    }

    /// <summary>
    /// Creates asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <returns></returns>
    /// <exception cref="System.Text.Json.JsonException"></exception>
    public async ValueTask<Shared.Models.Agent.ProjectMeta> CreateAsync(string agentUniqueName, string projectName)
    {
        try
        {
            CreateProjectResponse response = await _agentProjectManagementClient.CreateProjetAsync(new CreateProjectRequest
            {
                AgentUniqueName = agentUniqueName,
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
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryDeleteAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta)
    {
        try
        {
            _ = await _agentProjectManagementClient.DeleteProjectAsync(new DeleteProjectRequest
            {
                AgentUniqueName = agentUniqueName,
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
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryActivateAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta)
    {
        try
        {
            _ = await _agentProjectManagementClient.ActivateProjectAsync(new ActivateProjectRequest
            {
                AgentUniqueName = agentUniqueName,
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
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <param name="projectSaveInfo">The project save information.</param>
    public async ValueTask<bool> TrySaveAsync(string agentUniqueName,
                                                Shared.Models.Agent.ProjectMeta projectMeta,
                                                Shared.Models.Agent.ProjectSaveInfo projectSaveInfo)
    {
        try
        {
            _ = await _agentProjectManagementClient.SaveProjectAsync(new SaveProjectRequest
            {
                AgentUniqueName = agentUniqueName,
                ProjectDbId = projectMeta.DbId,
                ProjectId = projectMeta.Id,
                ProjectSaveInfo = CreateRpcProjectSaveInfo(projectSaveInfo)
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
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="dbId">The database identifier.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryApproveAsync(string agentUniqueName,
                                                    string dbId,
                                                    Shared.Models.Agent.ProjectSaveInfo projectSaveInfo)
    {
        try
        {
            _ = await _agentProjectManagementClient.ApproveProjectAsync(new ApproveProjectRequest
            {
                AgentUniqueName = agentUniqueName,
                ProjectDbId = dbId,
                ProjectSaveInfo = CreateRpcProjectSaveInfo(projectSaveInfo)
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to approve project");
            return false;
        }
    }

    private static ProjectSaveInfo CreateRpcProjectSaveInfo(Shared.Models.Agent.ProjectSaveInfo projectSaveInfo)
    {
        return new ProjectSaveInfo
        {
            State = (int)projectSaveInfo.State,
            VersionName = projectSaveInfo.VersionName,
            Comment = projectSaveInfo.Comment,
            UserName = projectSaveInfo.UserName
        };
    }
}
