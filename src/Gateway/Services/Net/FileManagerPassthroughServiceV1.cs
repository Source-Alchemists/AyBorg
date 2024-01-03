
using Ayborg.Gateway.Net.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Net;

public sealed class FileManagerPassthroughServiceV1 : FileManager.FileManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public FileManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<Empty> UploadImage(IAsyncStreamReader<ImageUploadRequest> requestStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            FileManager.FileManagerClient client = _channelService.CreateClient<FileManager.FileManagerClient>(channel.ServiceUniqueName);
            AsyncClientStreamingCall<ImageUploadRequest, Empty> requestCall = client.UploadImage(headers: headers, cancellationToken: context.CancellationToken);
            await foreach (ImageUploadRequest? imageChunk in requestStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await requestCall.RequestStream.WriteAsync(imageChunk, context.CancellationToken);
            }

            await requestCall.RequestStream.CompleteAsync();
            await requestCall;
        });

        return new Empty();
    }

    public override async Task<Empty> ConfirmUpload(ConfirmUploadRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);
        foreach (ChannelInfo channel in channels)
        {
            FileManager.FileManagerClient client = _channelService.CreateClient<FileManager.FileManagerClient>(channel.ServiceUniqueName);
            await client.ConfirmUploadAsync(request, headers);
        }

        return new Empty();
    }
}