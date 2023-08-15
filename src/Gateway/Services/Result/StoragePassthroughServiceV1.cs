using Ayborg.Gateway.Result.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.System;
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
            AsyncClientStreamingCall<ImageChunkDto, Empty> requestCall = client.AddImage(cancellationToken: token);
            await foreach (ImageChunkDto? imageChunk in requestStream.ReadAllAsync(cancellationToken: token))
            {
                await requestCall.RequestStream.WriteAsync(imageChunk, token);
            }

            await requestCall.RequestStream.CompleteAsync();
        });

        return new Empty();
    }
}
