/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Ayborg.Gateway.Agent.V1;
using AyBorg.Authorization;
using AyBorg.Data.Agent;
using AyBorg.Runtime.Projects;
using AyBorg.SDK.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class ProjectManagementServiceV1 : ProjectManagement.ProjectManagementBase
{
    private readonly IProjectManagementService _projectManagementService;

    public ProjectManagementServiceV1(IProjectManagementService projectManagementService)
    {
        _projectManagementService = projectManagementService;
    }

    public override async Task<GetProjectMetasResponse> GetProjectMetas(GetProjectMetasRequest request, ServerCallContext context)
    {
        var result = new GetProjectMetasResponse();
        foreach (IGrouping<Guid, ProjectMetaRecord> metaGroup in (await _projectManagementService.GetAllMetasAsync())
                                                                .Where(x => x.ServiceUniqueName.Equals(request.AgentUniqueName, StringComparison.InvariantCultureIgnoreCase))
                                                                .GroupBy(p => p.Id))
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

    public override async Task<Empty> ActivateProject(ActivateProjectRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProjectDbId"));
        }
        ProjectManagementResult result = await _projectManagementService.TryChangeActivationStateAsync(dbId, true);
        if (!result.IsSuccessful)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Message!));
        }

        return new Empty();
    }

    public override async Task<Empty> ApproveProject(ApproveProjectRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Reviewer });
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProjectDbId"));
        }
        ProjectSaveInfo saveInfo = request.ProjectSaveInfo;
        ProjectManagementResult result = await _projectManagementService.TrySaveAsync(dbId,
                                                                                                ProjectState.Ready,
                                                                                                saveInfo.VersionName,
                                                                                                saveInfo.UserName,
                                                                                                saveInfo.Comment);
        if (!result.IsSuccessful)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Message!));
        }

        return new Empty();
    }

    public override async Task<CreateProjectResponse> CreateProject(CreateProjectRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        ProjectRecord projectRecord = await _projectManagementService.CreateAsync(request.ProjectName);
        return new CreateProjectResponse
        {
            ProjectMeta = CreateProjectMeta(projectRecord.Meta)
        };
    }

    public override async Task<Empty> DeleteProject(DeleteProjectRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
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

    public override async Task<Empty> SaveProject(SaveProjectRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        ProjectManagementResult result;
        ProjectSaveInfo saveInfo = request.ProjectSaveInfo;
        var state = (ProjectState)saveInfo.State;
        if (!string.IsNullOrEmpty(request.ProjectId) && state == ProjectState.Draft)
        {
            if (!Guid.TryParse(request.ProjectId, out Guid id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProjectId"));
            }

            if (_projectManagementService.ActiveProjectId == id)
            {
                IEnumerable<ProjectMetaRecord> allProjects = await _projectManagementService.GetAllMetasAsync();
                ProjectMetaRecord? activeProject = allProjects.First(x => x.Id.Equals(_projectManagementService.ActiveProjectId));
                if (activeProject.State == ProjectState.Draft)
                {
                    // Save active project
                    result = await _projectManagementService.TrySaveActiveAsync(request.ProjectSaveInfo.UserName);
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

    private static Ayborg.Gateway.Agent.V1.ProjectMeta CreateProjectMeta(ProjectMetaRecord record)
    {
        return new Ayborg.Gateway.Agent.V1.ProjectMeta
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
        var state = (ProjectState)saveInfo.State;

        return await _projectManagementService.TrySaveAsync(dbId,
                                                                        state,
                                                                        saveInfo.VersionName,
                                                                        saveInfo.UserName,
                                                                        saveInfo.Comment);
    }
}
