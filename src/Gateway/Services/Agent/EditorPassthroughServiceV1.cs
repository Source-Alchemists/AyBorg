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

public sealed class EditorPassthroughServiceV1 : Editor.EditorBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public EditorPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetAvailableStepsResponse> GetAvailableSteps(GetAvailableStepsRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.GetAvailableStepsAsync(request);
    }

    public override async Task<GetFlowStepsResponse> GetFlowSteps(GetFlowStepsRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.GetFlowStepsAsync(request);
    }

    public override async Task<GetFlowLinksResponse> GetFlowLinks(GetFlowLinksRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.GetFlowLinksAsync(request);
    }

    public override async Task<GetFlowPortsResponse> GetFlowPorts(GetFlowPortsRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.GetFlowPortsAsync(request);
    }

    public override async Task<AddFlowStepResponse> AddFlowStep(AddFlowStepRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.AddFlowStepAsync(request, headers);
    }

    public override async Task<Empty> DeleteFlowStep(DeleteFlowStepRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.DeleteFlowStepAsync(request, headers);
    }

    public override async Task<Empty> MoveFlowStep(MoveFlowStepRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.MoveFlowStepAsync(request, headers);
    }

    public override async Task<LinkFlowPortsResponse> LinkFlowPorts(LinkFlowPortsRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.LinkFlowPortsAsync(request, headers);
    }

    public override async Task<Empty> UpdateFlowPort(UpdateFlowPortRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.UpdateFlowPortAsync(request, headers);
    }

    public override async Task GetImageStream(GetImageStreamRequest request, IServerStreamWriter<ImageChunkDto> responseStream, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        AsyncServerStreamingCall<ImageChunkDto> stream = client.GetImageStream(request);
        await foreach (ImageChunkDto? chunk in stream.ResponseStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(chunk);
        }
    }
}
