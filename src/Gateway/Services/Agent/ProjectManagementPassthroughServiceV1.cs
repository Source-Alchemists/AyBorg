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
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class ProjectManagementPassthroughServiceV1 : ProjectManagement.ProjectManagementBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public ProjectManagementPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetProjectMetasResponse> GetProjectMetas(GetProjectMetasRequest request, ServerCallContext context)
    {
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.GetProjectMetasAsync(request);
    }

    public override async Task<Empty> ActivateProject(ActivateProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.ActivateProjectAsync(request, headers);
    }

    public override async Task<Empty> ApproveProject(ApproveProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Reviewer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.ApproveProjectAsync(request, headers);
    }

    public override async Task<CreateProjectResponse> CreateProject(CreateProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.CreateProjectAsync(request, headers);
    }

    public override async Task<Empty> DeleteProject(DeleteProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.DeleteProjectAsync(request, headers);
    }

    public override async Task<Empty> SaveProject(SaveProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.SaveProjectAsync(request, headers);
    }
}
