
using Ayborg.Gateway.Audit.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Audit.Services;

public sealed class AuditServiceV1 : Ayborg.Gateway.Audit.V1.Audit.AuditBase
{
    public override Task<Empty> AddEntry(AuditEntry request, ServerCallContext context)
    {
        switch(request.PayloadCase)
        {
            case AuditEntry.PayloadOneofCase.AgentProject:
            break;
        }
        return Task.FromResult(new Empty());
    }

    public override Task GetEntries(GetAuditEntriesRequest request, IServerStreamWriter<AuditEntry> responseStream, ServerCallContext context)
    {
        return Task.CompletedTask;
    }

    public override Task GetReportMetas(GetAuditReportMetasRequest request, IServerStreamWriter<AuditReportMeta> responseStream, ServerCallContext context)
    {
        return Task.CompletedTask;
    }
}
