
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

    public override async Task<ImageCollectionMeta> GetImageCollectionMeta(GetImageCollectionMetaRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);
        ImageCollectionMeta clientResponse = null!;
        foreach (ChannelInfo channel in channels)
        {
            FileManager.FileManagerClient client = _channelService.CreateClient<FileManager.FileManagerClient>(channel.ServiceUniqueName);
            clientResponse = await client.GetImageCollectionMetaAsync(request, headers);
        }

        return clientResponse;
    }

    public override async Task DownloadImage(ImageDownloadRequest request, IServerStreamWriter<ImageChunk> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            FileManager.FileManagerClient client = _channelService.CreateClient<FileManager.FileManagerClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<ImageChunk> requestCall = client.DownloadImage(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (ImageChunk imageChunk in requestCall.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(imageChunk, cancellationToken: context.CancellationToken);
            }
        });
    }

    public override async Task GetModelMetas(GetModelMetasRequest request, IServerStreamWriter<ModelMeta> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), null!);
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            FileManager.FileManagerClient client = _channelService.CreateClient<FileManager.FileManagerClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<ModelMeta> requestCall = client.GetModelMetas(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (ModelMeta meta in requestCall.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(meta, cancellationToken: context.CancellationToken);
            }
        });
    }

    public override async Task<Empty> EditModel(EditModelRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);
        foreach (ChannelInfo channel in channels)
        {
            FileManager.FileManagerClient client = _channelService.CreateClient<FileManager.FileManagerClient>(channel.ServiceUniqueName);
            await client.EditModelAsync(request, headers);
        }

        return new Empty();
    }

    public override async Task<Empty> DeleteModel(DeleteModelRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);
        foreach (ChannelInfo channel in channels)
        {
            FileManager.FileManagerClient client = _channelService.CreateClient<FileManager.FileManagerClient>(channel.ServiceUniqueName);
            await client.DeleteModelAsync(request, headers);
        }

        return new Empty();
    }

    public override async Task<Empty> ChangeModelState(ChangeModelStateRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);
        foreach (ChannelInfo channel in channels)
        {
            FileManager.FileManagerClient client = _channelService.CreateClient<FileManager.FileManagerClient>(channel.ServiceUniqueName);
            await client.ChangeModelStateAsync(request, headers);
        }

        return new Empty();
    }
}
