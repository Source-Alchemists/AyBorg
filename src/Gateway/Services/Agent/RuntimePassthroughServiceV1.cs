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
using AGB = Ayborg.Gateway.Agent.V1.Runtime;

using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class RuntimePassthroughServiceV1 : AGB.RuntimeBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public RuntimePassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetRuntimeStatusResponse> GetStatus(GetRuntimeStatusRequest request, ServerCallContext context)
    {
        AGB.RuntimeClient client = _grpcChannelService.CreateClient<AGB.RuntimeClient>(request.AgentUniqueName);
        return await client.GetStatusAsync(request);
    }

    public override async Task<StartRunResponse> StartRun(StartRunRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        AGB.RuntimeClient client = _grpcChannelService.CreateClient<AGB.RuntimeClient>(request.AgentUniqueName);
        return await client.StartRunAsync(request, headers);
    }

    public override async Task<StopRunResponse> StopRun(StopRunRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        AGB.RuntimeClient client = _grpcChannelService.CreateClient<AGB.RuntimeClient>(request.AgentUniqueName);
        return await client.StopRunAsync(request, headers);
    }

    public override async Task<AbortRunResponse> AbortRun(AbortRunRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        AGB.RuntimeClient client = _grpcChannelService.CreateClient<AGB.RuntimeClient>(request.AgentUniqueName);
        return await client.AbortRunAsync(request, headers);
    }
}
