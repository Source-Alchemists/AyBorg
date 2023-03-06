using Ayborg.Gateway.Audit.V1;
using AyBorg.Web.Shared.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Web.Services;

public sealed class AuditService : IAuditService
{
    private readonly Audit.AuditClient _auditClient;

    public AuditService(Audit.AuditClient auditClient)
    {
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
}
