using Ayborg.Gateway.Cognitive.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Cognitive;

public sealed class ProjectManagerPassthroughServiceV1 : ProjectManager.ProjectManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public ProjectManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<ProjectMeta> Create(CreateProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);
        ProjectMeta lastResponse = null!;
        foreach (ChannelInfo channel in channels)
        {
            ProjectManager.ProjectManagerClient client = _channelService.CreateClient<ProjectManager.ProjectManagerClient>(channel.ServiceUniqueName);
            lastResponse = await client.CreateAsync(request, headers);
        }
        return lastResponse;
    }

    public override async Task<Empty> Delete(ProjectMeta request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);
        foreach (ChannelInfo channel in channels)
        {
            ProjectManager.ProjectManagerClient client = _channelService.CreateClient<ProjectManager.ProjectManagerClient>(channel.ServiceUniqueName);
            await client.DeleteAsync(request, headers);
        }

        return new Empty();
    }

    public override async Task GetMetas(Empty request, IServerStreamWriter<ProjectMeta> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            ProjectManager.ProjectManagerClient client = _channelService.CreateClient<ProjectManager.ProjectManagerClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<ProjectMeta> response = client.GetMetas(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (ProjectMeta? projectMeta in response.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(projectMeta, cancellationToken: context.CancellationToken);
            }
        });
    }

    public override async Task<ClassLabel> AddOrUpdateClassLabel(AddOrUpdateClassLabelRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

        ClassLabel response = null!;

        foreach (ChannelInfo channel in channels)
        {
            ProjectManager.ProjectManagerClient client = _channelService.CreateClient<ProjectManager.ProjectManagerClient>(channel.ServiceUniqueName);
            response = await client.AddOrUpdateClassLabelAsync(request, headers);
        }

        return response;
    }
}
