using Ayborg.Gateway.Cognitive.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Cognitive;

public sealed class DatasetManagerPassthroughServiceV1 : DatasetManager.DatasetManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public DatasetManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<Empty> AddImage(AddImageRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        foreach (ChannelInfo channel in channels)
        {
            DatasetManager.DatasetManagerClient client = _channelService.CreateClient<DatasetManager.DatasetManagerClient>(channel.ServiceUniqueName);
            await client.AddImageAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return new Empty();
    }

    public override async Task<DatasetMeta> Create(CreateRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        DatasetMeta result = new();
        foreach (ChannelInfo channel in channels)
        {
            DatasetManager.DatasetManagerClient client = _channelService.CreateClient<DatasetManager.DatasetManagerClient>(channel.ServiceUniqueName);
            result = await client.CreateAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return result;
    }

    public override async Task<Empty> Delete(DeleteRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        foreach (ChannelInfo channel in channels)
        {
            DatasetManager.DatasetManagerClient client = _channelService.CreateClient<DatasetManager.DatasetManagerClient>(channel.ServiceUniqueName);
            await client.DeleteAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return new Empty();
    }

    public override async Task<Empty> Edit(EditRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        foreach (ChannelInfo channel in channels)
        {
            DatasetManager.DatasetManagerClient client = _channelService.CreateClient<DatasetManager.DatasetManagerClient>(channel.ServiceUniqueName);
            await client.EditAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return new Empty();
    }

    public override async Task<Empty> Generate(GenerateRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        foreach (ChannelInfo channel in channels)
        {
            DatasetManager.DatasetManagerClient client = _channelService.CreateClient<DatasetManager.DatasetManagerClient>(channel.ServiceUniqueName);
            await client.GenerateAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return new Empty();
    }

    public override async Task GetMetas(GetMetasRequest request, IServerStreamWriter<DatasetMeta> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            DatasetManager.DatasetManagerClient client = _channelService.CreateClient<DatasetManager.DatasetManagerClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<DatasetMeta> response = client.GetMetas(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (DatasetMeta datasetMeta in response.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(datasetMeta, cancellationToken: context.CancellationToken);
            }
        });
    }

    public override async Task GetImageNames(GetImagesRequest request, IServerStreamWriter<ImageInfo> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            DatasetManager.DatasetManagerClient client = _channelService.CreateClient<DatasetManager.DatasetManagerClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<ImageInfo> response = client.GetImageNames(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (ImageInfo datasetMeta in response.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(datasetMeta, cancellationToken: context.CancellationToken);
            }
        });
    }
}
