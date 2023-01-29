
using Ayborg.Gateway.Audit.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Audit.Services;

public sealed class AuditServiceV1 : Ayborg.Gateway.Audit.V1.Audit.AuditBase
{
    private readonly AgentMapper _agentMapper;
    private readonly IAgentAuditService _agentAuditService;

    public AuditServiceV1(AgentMapper agentMapper, IAgentAuditService agentAuditService)
    {
        _agentMapper = agentMapper;
        _agentAuditService = agentAuditService;
    }

    public override Task<Empty> AddEntry(AuditEntry request, ServerCallContext context)
    {
        switch (request.PayloadCase)
        {
            case AuditEntry.PayloadOneofCase.AgentProject:
                Data.Audit.Models.Agent.ProjectAuditRecord projectAuditRecord = _agentMapper.Map(request.AgentProject);
                if (!_agentAuditService.TryAdd(projectAuditRecord))
                {
                    throw new RpcException(Status.DefaultCancelled, "Failed to add project audit entry.");
                }
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
