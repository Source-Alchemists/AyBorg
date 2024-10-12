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
using AyBorg.SDK.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class ProjectSettingsServiceV1 : ProjectSettings.ProjectSettingsBase
{
    private readonly IProjectSettingsService _projectSettingsService;

    public ProjectSettingsServiceV1(IProjectSettingsService projectSettingsService)
    {
        _projectSettingsService = projectSettingsService;
    }

    public override async Task<GetProjectSettingsResponse> GetProjectSettings(GetProjectSettingsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectDbId is not a valid GUID"));

        }
        ProjectSettingsRecord projectSettingsRecord = await _projectSettingsService.GetSettingsRecordAsync(dbId);

        return new GetProjectSettingsResponse
        {
            ProjectSettings = new ProjectSettingsDto
            {
                IsForceResultCommunicationEnabled = projectSettingsRecord.IsForceResultCommunicationEnabled
            }
        };
    }

    public override async Task<Empty> UpdateProjectSettings(UpdateProjectSettingsRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator });
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectDbId is not a valid GUID"));
        }

        ProjectSettingsDto settings = request.ProjectSettings;

        if (!await _projectSettingsService.TryUpdateActiveProjectSettingsAsync(dbId, new AyBorg.Runtime.Projects.ProjectSettings
        {
            IsForceResultCommunicationEnabled = settings.IsForceResultCommunicationEnabled
        }))
        {
            throw new RpcException(new Status(StatusCode.Internal, "Could not update project settings"));
        }

        return new Empty();
    }
}
