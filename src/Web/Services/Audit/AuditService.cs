using Ayborg.Gateway.Audit.V1;
using AyBorg.SDK.Common;
using AyBorg.Web.Shared.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

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

    public async IAsyncEnumerable<Shared.Models.AuditChangeset> GetAuditChangesetsAsync()
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

    public async IAsyncEnumerable<Shared.Models.AuditChange> GetAuditChangesAsync(IEnumerable<Shared.Models.AuditChangeset> changesets)
    {
        var request = new GetAuditChangesRequest();
        request.Tokens.AddRange(changesets.Select(c => c.Token.ToString()).ToList());
        AsyncServerStreamingCall<AuditChange> response = _auditClient.GetChanges(request);

        await foreach (AuditChange? change in response.ResponseStream.ReadAllAsync())
        {
            yield return AuditMapper.Map(change);
        }
    }

    public async ValueTask<bool> TrySaveReport(string reportName, string comment, IEnumerable<Shared.Models.AuditChangeset> changesets)
    {
        var request = new SaveAuditReportRequest
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
            await _auditClient.SaveReportAsync(request);
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to save report");
            return false;
        }
    }
}
