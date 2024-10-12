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

using Ayborg.Gateway.Audit.V1;
using AyBorg.Gateway.Models;
using AyBorg.Communication;
using AyBorg.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Audit;

public sealed class AuditPassthroughServiceV1 : Ayborg.Gateway.Audit.V1.Audit.AuditBase
{
    private readonly IGrpcChannelService _channelService;
    private readonly IGatewayConfiguration _configuration;

    public AuditPassthroughServiceV1(IGrpcChannelService grpcChannelService, IGatewayConfiguration configuration)
    {
        _channelService = grpcChannelService;
        _configuration = configuration;
    }

    public override async Task<Empty> AddEntry(AuditEntry request, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Audit);
        ThrowIfAuditRequiredButNotAvailable(channels);

        foreach (ChannelInfo channel in channels)
        {
            Ayborg.Gateway.Audit.V1.Audit.AuditClient client = _channelService.CreateClient<Ayborg.Gateway.Audit.V1.Audit.AuditClient>(channel.ServiceUniqueName);
            await client.AddEntryAsync(request);
        }

        return new Empty();
    }

    public override async Task<Empty> InvalidateEntry(InvalidateAuditEntryRequest request, ServerCallContext context)
    {
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Audit);
        ThrowIfAuditRequiredButNotAvailable(channels);

        foreach (ChannelInfo channel in channels)
        {
            Ayborg.Gateway.Audit.V1.Audit.AuditClient client = _channelService.CreateClient<Ayborg.Gateway.Audit.V1.Audit.AuditClient>(channel.ServiceUniqueName);
            await client.InvalidateEntryAsync(request);
        }

        return new Empty();
    }

    public override async Task GetChangesets(GetAuditChangesetsRequest request, IServerStreamWriter<AuditChangeset> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Auditor });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Audit);
        ThrowIfAuditRequiredButNotAvailable(channels);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            Ayborg.Gateway.Audit.V1.Audit.AuditClient client = _channelService.CreateClient<Ayborg.Gateway.Audit.V1.Audit.AuditClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<AuditChangeset> response = client.GetChangesets(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (AuditChangeset? changeset in response.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(changeset, cancellationToken: context.CancellationToken);
            }
        });
    }

    public override async Task GetChanges(GetAuditChangesRequest request, IServerStreamWriter<AuditChange> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Auditor });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Audit);
        ThrowIfAuditRequiredButNotAvailable(channels);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            Ayborg.Gateway.Audit.V1.Audit.AuditClient client = _channelService.CreateClient<Ayborg.Gateway.Audit.V1.Audit.AuditClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<AuditChange> response = client.GetChanges(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (AuditChange? change in response.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(change, cancellationToken: context.CancellationToken);
            }
        });
    }

    public override async Task<Empty> AddReport(AddAuditReportRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Auditor });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Audit);
        ThrowIfAuditRequiredButNotAvailable(channels);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            Ayborg.Gateway.Audit.V1.Audit.AuditClient client = _channelService.CreateClient<Ayborg.Gateway.Audit.V1.Audit.AuditClient>(channel.ServiceUniqueName);
            await client.AddReportAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        });

        return new Empty();
    }

    public override async Task GetReports(Empty request, IServerStreamWriter<AuditReport> responseStream, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Auditor });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Audit);
        ThrowIfAuditRequiredButNotAvailable(channels);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            Ayborg.Gateway.Audit.V1.Audit.AuditClient client = _channelService.CreateClient<Ayborg.Gateway.Audit.V1.Audit.AuditClient>(channel.ServiceUniqueName);
            AsyncServerStreamingCall<AuditReport> response = client.GetReports(request, headers: headers, cancellationToken: context.CancellationToken);
            await foreach (AuditReport? report in response.ResponseStream.ReadAllAsync(cancellationToken: context.CancellationToken))
            {
                await responseStream.WriteAsync(report, cancellationToken: context.CancellationToken);
            }
        });
    }

    public override async Task<Empty> DeleteReport(AuditReport request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator });
        IEnumerable<ChannelInfo> channels = _channelService.GetChannelsByTypeName(ServiceTypes.Audit);
        ThrowIfAuditRequiredButNotAvailable(channels);

        await Parallel.ForEachAsync(channels, async (channel, token) =>
        {
            Ayborg.Gateway.Audit.V1.Audit.AuditClient client = _channelService.CreateClient<Ayborg.Gateway.Audit.V1.Audit.AuditClient>(channel.ServiceUniqueName);
            await client.DeleteReportAsync(request, headers: headers, cancellationToken: context.CancellationToken);
        });

        return new Empty();
    }

    private void ThrowIfAuditRequiredButNotAvailable(IEnumerable<ChannelInfo> channels)
    {
        if (_configuration.IsAuditRequired && !channels.Any())
        {
            throw new RpcException(Status.DefaultCancelled, "Audit is required but no audit service is available.");
        }
    }
}
