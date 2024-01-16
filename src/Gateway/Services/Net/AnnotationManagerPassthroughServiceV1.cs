using Ayborg.Gateway.Net.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Net;

public sealed class AnnotationManagerPassthroughServiceV1 : AnnotationManager.AnnotationManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public AnnotationManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<Meta> GetMeta(GetMetaRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        Meta result = new();

        foreach (ChannelInfo channel in channels)
        {
            AnnotationManager.AnnotationManagerClient client = _channelService.CreateClient<AnnotationManager.AnnotationManagerClient>(channel.ServiceUniqueName);
            result = await client.GetMetaAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return result;
    }

    public override async Task<Layer> Get(GetRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        Layer result = new();
        foreach (ChannelInfo channel in channels)
        {
            AnnotationManager.AnnotationManagerClient client = _channelService.CreateClient<AnnotationManager.AnnotationManagerClient>(channel.ServiceUniqueName);
            result = await client.GetAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return result;
    }

    public override async Task<Empty> Add(AddRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        foreach (ChannelInfo channel in channels)
        {
            AnnotationManager.AnnotationManagerClient client = _channelService.CreateClient<AnnotationManager.AnnotationManagerClient>(channel.ServiceUniqueName);
            await client.AddAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return new Empty();
    }
    public override async Task<Empty> Remove(RemoveRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        foreach (ChannelInfo channel in channels)
        {
            AnnotationManager.AnnotationManagerClient client = _channelService.CreateClient<AnnotationManager.AnnotationManagerClient>(channel.ServiceUniqueName);
            await client.RemoveAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        }

        return new Empty();
    }
}