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

using Ayborg.Gateway.Cognitive.V1;
using AyBorg.Authorization;
using AyBorg.Communication;
using AyBorg.Gateway.Models;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Cognitive;

public sealed class AnnotationManagerPassthroughServiceV1 : AnnotationManager.AnnotationManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public AnnotationManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<AnnotationMeta> GetMeta(GetAnnotationMetaRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        AnnotationMeta result = new();

        foreach (ChannelInfo channel in channels)
        {
            AnnotationManager.AnnotationManagerClient client = _channelService.CreateClient<AnnotationManager.AnnotationManagerClient>(channel.ServiceUniqueName);
            result = await client.GetMetaAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return result;
    }

    public override async Task<AnnotationLayer> Get(GetAnnotationRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        AnnotationLayer result = new();
        foreach (ChannelInfo channel in channels)
        {
            AnnotationManager.AnnotationManagerClient client = _channelService.CreateClient<AnnotationManager.AnnotationManagerClient>(channel.ServiceUniqueName);
            result = await client.GetAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return result;
    }

    public override async Task<Empty> Add(AddAnnotationRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        foreach (ChannelInfo channel in channels)
        {
            AnnotationManager.AnnotationManagerClient client = _channelService.CreateClient<AnnotationManager.AnnotationManagerClient>(channel.ServiceUniqueName);
            await client.AddAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return new Empty();
    }
    public override async Task<Empty> Remove(RemoveAnnotationRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        foreach (ChannelInfo channel in channels)
        {
            AnnotationManager.AnnotationManagerClient client = _channelService.CreateClient<AnnotationManager.AnnotationManagerClient>(channel.ServiceUniqueName);
            await client.RemoveAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return new Empty();
    }
}
