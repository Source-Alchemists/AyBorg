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

using Ayborg.Gateway.Result.V1;
using AyBorg.Communication;
using AyBorg.Gateway.Models;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Result;

public sealed class StoragePassthroughServiceV1 : Storage.StorageBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public StoragePassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<Empty> Add(AddRequest request, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _grpcChannelService.GetChannelsByTypeName(ServiceTypes.Result);
        foreach (ChannelInfo channel in channels)
        {
            Storage.StorageClient client = _grpcChannelService.CreateClient<Storage.StorageClient>(channel.ServiceUniqueName);
            await client.AddAsync(request);
        }

        return new Empty();
    }

    public override async Task<Empty> AddImage(IAsyncStreamReader<ImageChunkDto> requestStream, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _grpcChannelService.GetChannelsByTypeName(ServiceTypes.Result);
        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            Storage.StorageClient client = _grpcChannelService.CreateClient<Storage.StorageClient>(channel.ServiceUniqueName);
            using AsyncClientStreamingCall<ImageChunkDto, Empty> requestCall = client.AddImage(cancellationToken: context.CancellationToken);
            await foreach (ImageChunkDto? imageChunk in requestStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await requestCall.RequestStream.WriteAsync(imageChunk, context.CancellationToken);
            }

            await requestCall.RequestStream.CompleteAsync();
            await requestCall;
        });

        return new Empty();
    }
}
