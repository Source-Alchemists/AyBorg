
using Ayborg.Gateway.Net.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Net;

public sealed class FilemanagerPassthroughServiceV1 : Filemanager.FilemanagerBase
{
    private readonly IGrpcChannelService _channelService;

    public FilemanagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<Empty> UploadImage(IAsyncStreamReader<ImageUploadRequest> requestStream, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            Filemanager.FilemanagerClient client = _channelService.CreateClient<Filemanager.FilemanagerClient>(channel.ServiceUniqueName);
            AsyncClientStreamingCall<ImageUploadRequest, Empty> requestCall = client.UploadImage(cancellationToken: context.CancellationToken);
            await foreach (ImageUploadRequest? imageChunk in requestStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await requestCall.RequestStream.WriteAsync(imageChunk, context.CancellationToken);
            }

            await requestCall.RequestStream.CompleteAsync();
            await requestCall;
        });

        return new Empty();
    }
}