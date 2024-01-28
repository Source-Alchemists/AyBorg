using Ayborg.Gateway.Net.V1;
using AyBorg.Gateway.Models;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Net;

public sealed class JobManagerPassthroughServiceV1 : JobManager.JobManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public JobManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task GetMetas(Empty request, IServerStreamWriter<JobMeta> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            JobManager.JobManagerClient client = _channelService.CreateClient<JobManager.JobManagerClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<JobMeta> response = client.GetMetas(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (JobMeta jobMeta in response.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(jobMeta, cancellationToken: context.CancellationToken);
            }
        });
    }

    public override async Task<Job> Get(GetJobRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Auditor, Roles.Reviewer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);
        Job lastResponse = null!;
        foreach (ChannelInfo channel in channels)
        {
            JobManager.JobManagerClient client = _channelService.CreateClient<JobManager.JobManagerClient>(channel.ServiceUniqueName);
            lastResponse = await client.GetAsync(request, headers);
        }

        return lastResponse;
    }

    public override async Task<Empty> Create(CreateJobRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);
        foreach (ChannelInfo channel in channels)
        {
            JobManager.JobManagerClient client = _channelService.CreateClient<JobManager.JobManagerClient>(channel.ServiceUniqueName);
            await client.CreateAsync(request, headers);
        }

        return new Empty();
    }

    public override async Task<Empty> Cancel(CancelJobRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Net);
        foreach (ChannelInfo channel in channels)
        {
            JobManager.JobManagerClient client = _channelService.CreateClient<JobManager.JobManagerClient>(channel.ServiceUniqueName);
            await client.CancelAsync(request, headers);
        }

        return new Empty();
    }
}
