using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Data.DAL;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class ProjectManagementServiceV1 : ProjectManagement.ProjectManagementBase
{
    private readonly ILogger<ProjectManagementServiceV1> _logger;
    private readonly IProjectManagementService _projectManagementService;

    public ProjectManagementServiceV1(ILogger<ProjectManagementServiceV1> logger, IProjectManagementService projectManagementService)
    {
        _logger = logger;
        _projectManagementService = projectManagementService;
    }

    public override async Task<Empty> ActivateProject(ActivateProjectRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProjectDbId"));
        }
        ProjectManagementResult result = await _projectManagementService.TryActivateAsync(dbId, true);
        if (!result.IsSuccessful)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Message!));
        }

        return new Empty();
    }

    public override async Task<Empty> ApproveProject(ApproveProjectRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProjectDbId"));
        }
        ProjectSaveInfo saveInfo = request.ProjectSaveInfo;
        ProjectManagementResult result = await _projectManagementService.TrySaveNewVersionAsync(dbId,
                                                                                                SDK.Projects.ProjectState.Ready,
                                                                                                saveInfo.VersionName,
                                                                                                saveInfo.Comment,
                                                                                                saveInfo.UserName);
        if (!result.IsSuccessful)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Message!));
        }

        return new Empty();
    }

    public override async Task<CreateProjectResponse> CreateProject(CreateProjectRequest request, ServerCallContext context)
    {
        ProjectRecord projectRecord = await _projectManagementService.CreateAsync(request.ProjectName);
        return new CreateProjectResponse
        {
            ProjectMeta = CreateProjectMeta(projectRecord.Meta)
        };
    }

    public override async Task<Empty> DeleteProject(DeleteProjectRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectId, out Guid id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProjectId"));
        }
        ProjectManagementResult result = await _projectManagementService.TryDeleteAsync(id);
        if (!result.IsSuccessful)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Message!));
        }

        return new Empty();
    }

    public override async Task<GetProjectMetasResponse> GetProjectMetas(GetProjectMetasRequest request, ServerCallContext context)
    {
        var result = new GetProjectMetasResponse();
        foreach (IGrouping<Guid, ProjectMetaRecord> metaGroup in (await _projectManagementService.GetAllMetasAsync()).Where(x => x.ServiceUniqueName == request.AgentUniqueName).GroupBy(p => p.Id))
        {
            ProjectMetaRecord? activeMeta = metaGroup.FirstOrDefault(g => g.IsActive);
            if (activeMeta != null)
            {
                result.ProjectMetas.Add(CreateProjectMeta(activeMeta));
            }
            else
            {
                ProjectMetaRecord meta = metaGroup.OrderByDescending(x => x.UpdatedDate).First();
                result.ProjectMetas.Add(CreateProjectMeta(meta));
            }
        }
        return result;
    }

    public override async Task<Empty> SaveProject(SaveProjectRequest request, ServerCallContext context)
    {
        ProjectManagementResult result;
        ProjectSaveInfo saveInfo = request.ProjectSaveInfo;
        var state = (SDK.Projects.ProjectState)saveInfo.State;
        if (!string.IsNullOrEmpty(request.ProjectId) && state == SDK.Projects.ProjectState.Draft)
        {
            if (!Guid.TryParse(request.ProjectId, out Guid id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProjectId"));
            }

            if (_projectManagementService.ActiveProjectId == id)
            {
                IEnumerable<ProjectMetaRecord> allProjects = await _projectManagementService.GetAllMetasAsync();
                ProjectMetaRecord? activeProject = allProjects.First(x => x.Id.Equals(_projectManagementService.ActiveProjectId));
                if (activeProject.State == SDK.Projects.ProjectState.Draft)
                {
                    // Save active project
                    result = await _projectManagementService.TrySaveActiveAsync();
                }
                else
                {
                    // Change project state
                    result = await ChangeProjectState(request);
                }
            }
            else
            {
                // Change project state
                result = await ChangeProjectState(request);
            }
        }
        else
        {
            // Change project state
            result = await ChangeProjectState(request);
        }

        if (!result.IsSuccessful)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Message!));
        }

        return new Empty();
    }

    private static ProjectMeta CreateProjectMeta(ProjectMetaRecord record)
    {
        return new ProjectMeta
        {
            DbId = record.DbId.ToString(),
            Id = record.Id.ToString(),
            Name = record.Name,
            VersionName = record.VersionName ?? string.Empty,
            Comment = record.Comment ?? string.Empty,
            CreationDate = Timestamp.FromDateTime(record.CreatedDate.ToUniversalTime()),
            ChangeDate = Timestamp.FromDateTime(record.UpdatedDate.ToUniversalTime()),
            IsActive = record.IsActive,
            State = (int)record.State,
            ApprovedBy = record.ApprovedBy ?? string.Empty
        };
    }

    private async ValueTask<ProjectManagementResult> ChangeProjectState(SaveProjectRequest request)
    {
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProjectDbId"));
        }
        ProjectSaveInfo saveInfo = request.ProjectSaveInfo;
        var state = (SDK.Projects.ProjectState)saveInfo.State;

        return await _projectManagementService.TrySaveNewVersionAsync(dbId,
                                                                        state,
                                                                        saveInfo.VersionName,
                                                                        saveInfo.Comment,
                                                                        saveInfo.UserName);
    }
}
