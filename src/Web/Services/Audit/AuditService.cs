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
using AyBorg.Types;
using AyBorg.Web.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Web.Services;

public sealed class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly Audit.AuditClient _auditClient;

    public AuditService(ILogger<AuditService> logger, Audit.AuditClient auditClient)
    {
        _logger = logger;
        _auditClient = auditClient;
    }

    public async IAsyncEnumerable<Shared.Models.AuditChangeset> GetChangesetsAsync()
    {
        AsyncServerStreamingCall<AuditChangeset> response = _auditClient.GetChangesets(new GetAuditChangesetsRequest
        {
            ServiceType = string.Empty,
            ServiceUniqueName = string.Empty,
            From = Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime()),
            To = Timestamp.FromDateTime(DateTime.MaxValue.ToUniversalTime())
        });

        await foreach (AuditChangeset? changeset in response.ResponseStream.ReadAllAsync())
        {
            yield return AuditMapper.Map(changeset);
        }
    }

    public async IAsyncEnumerable<Shared.Models.AuditChange> GetChangesAsync(IEnumerable<Shared.Models.AuditChangeset> changesets)
    {
        var request = new GetAuditChangesRequest();
        request.Tokens.AddRange(changesets.Select(c => c.Token.ToString()).ToList());
        AsyncServerStreamingCall<AuditChange> response = _auditClient.GetChanges(request);

        await foreach (AuditChange? change in response.ResponseStream.ReadAllAsync())
        {
            yield return AuditMapper.Map(change);
        }
    }

    public async ValueTask<bool> TryAddReport(string reportName, string comment, IEnumerable<Shared.Models.AuditChangeset> changesets)
    {
        var request = new AddAuditReportRequest
        {
            ReportName = reportName,
            Comment = comment
        };

        foreach (Shared.Models.AuditChangeset changeset in changesets)
        {
            request.Changesets.Add(AuditMapper.Map(changeset));
        }

        try
        {
            await _auditClient.AddReportAsync(request);
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to save report");
            return false;
        }
    }

    public async IAsyncEnumerable<Shared.Models.AuditReport> GetReportsAsync()
    {
        AsyncServerStreamingCall<AuditReport> response = _auditClient.GetReports(new Empty());

        await foreach (AuditReport? report in response.ResponseStream.ReadAllAsync())
        {
            yield return AuditMapper.Map(report);
        }
    }

    public async ValueTask<bool> TryDeleteReport(Shared.Models.AuditReport report)
    {
        try
        {
            await _auditClient.DeleteReportAsync(AuditMapper.Map(report));
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to delete report");
            return false;
        }
    }
}
