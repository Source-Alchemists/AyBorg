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

using Ayborg.Gateway.Cognitive.V1;
using AyBorg.Gateway.Models;
using AyBorg.Communication;
using AyBorg.Authorization;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Cognitive;

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
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);

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
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);
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
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);
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
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Cognitive);
        foreach (ChannelInfo channel in channels)
        {
            JobManager.JobManagerClient client = _channelService.CreateClient<JobManager.JobManagerClient>(channel.ServiceUniqueName);
            await client.CancelAsync(request, headers);
        }

        return new Empty();
    }
}
