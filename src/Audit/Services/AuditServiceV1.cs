
using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Audit.Models;
using AyBorg.Data.Audit.Models.Agent;
using AyBorg.Data.Audit.Repositories.Agent;
using AyBorg.SDK.System;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Audit.Services;

public sealed class AuditServiceV1 : Ayborg.Gateway.Audit.V1.Audit.AuditBase
{
    private readonly AgentMapper _agentMapper;
    private readonly IAgentAuditService _agentAuditService;
    private readonly IProjectAuditRepository _projectAuditRepository;
    private readonly ICompareService _compareService;

    public AuditServiceV1(AgentMapper agentMapper, IAgentAuditService agentAuditService, IProjectAuditRepository projectAuditRepository, ICompareService compareService)
    {
        _agentMapper = agentMapper;
        _agentAuditService = agentAuditService;
        _projectAuditRepository = projectAuditRepository;
        _compareService = compareService;
    }

    public override Task<Empty> AddEntry(AuditEntry request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            switch (request.PayloadCase)
            {
                case AuditEntry.PayloadOneofCase.AgentProject:
                    ProjectAuditRecord projectAuditRecord = _agentMapper.MapToProjectRecord(request);
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

    public override async Task GetChangesets(GetAuditChangesetsRequest request, IServerStreamWriter<AuditChangeset> responseStream, ServerCallContext context)
    {
        foreach (ChangesetRecord changeset in _agentAuditService.GetChangesets())
        {
            await responseStream.WriteAsync(_agentMapper.Map(changeset));
        }
    }

    public override async Task GetChanges(GetAuditChangesRequest request, IServerStreamWriter<AuditChange> responseStream, ServerCallContext context)
    {
        IEnumerable<ProjectAuditRecord> changesets = _projectAuditRepository.FindAll().Where(c => request.Tokens.Any(t => t.Equals(c.Id.ToString(), StringComparison.OrdinalIgnoreCase)));
        IEnumerable<ChangeRecord> changes = _compareService.Compare(changesets);
        foreach (ChangeRecord change in changes)
        {
            await responseStream.WriteAsync(_agentMapper.Map(change));
        }
    }

    // public override Task GetReportMetas(GetAuditReportMetasRequest request, IServerStreamWriter<AuditReportMeta> responseStream, ServerCallContext context)
    // {
    //     return Task.CompletedTask;
    // }
}
