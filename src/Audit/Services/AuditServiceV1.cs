
using Ayborg.Gateway.Audit.V1;
using AyBorg.SDK.System;
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
        return Task.Factory.StartNew(() =>
        {
            switch (request.PayloadCase)
            {
                case AuditEntry.PayloadOneofCase.AgentProject:
                    Data.Audit.Models.Agent.ProjectAuditRecord projectAuditRecord = _agentMapper.Map(request.AgentProject);
                    projectAuditRecord.Id = Guid.Parse(request.Token);
                    projectAuditRecord.SavedBy = request.User;
                    if (!_agentAuditService.TryAdd(projectAuditRecord))
                    {
                        throw new RpcException(new Status(StatusCode.DataLoss, "Failed to add project audit entry."));
                    }
                    break;
            }

            return new Empty();
        });
    }

    public override Task<Empty> InvalidateEntry(InvalidateAuditEntryRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            if (request.ServiceType.Equals(ServiceTypes.Agent))
            {
                if (!_agentAuditService.TryRemove(Guid.Parse(request.Token)))
                {
                    throw new RpcException(new Status(StatusCode.Internal, "Failed to invalidate audit entry."));
                }
            }
            else
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid service type."));
            }

            return new Empty();
        });
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
