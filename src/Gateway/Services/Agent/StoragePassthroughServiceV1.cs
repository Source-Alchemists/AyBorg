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
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class StoragePassthroughServiceV1 : Storage.StorageBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public StoragePassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetDirectoriesResponse> GetDirectories(GetDirectoriesRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        Storage.StorageClient client = _grpcChannelService.CreateClient<Storage.StorageClient>(request.AgentUniqueName);
        return await client.GetDirectoriesAsync(request, headers);
    }
}
