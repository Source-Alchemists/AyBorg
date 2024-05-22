using Ayborg.Gateway.Cognitive.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System;
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
