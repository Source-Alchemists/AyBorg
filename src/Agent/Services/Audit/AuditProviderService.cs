using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.System;
using AyBorg.SDK.System.Configuration;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services;

public sealed class AuditProviderService : IAuditProviderService
{
    private readonly ILogger<AuditProviderService> _logger;
    private readonly Audit.AuditClient _auditClient;
    private readonly AuditMapper _auditMapper;
    private readonly IServiceConfiguration _serviceConfiguration;

    public AuditProviderService(ILogger<AuditProviderService> logger, Audit.AuditClient auditClient, AuditMapper auditMapper, IServiceConfiguration serviceConfiguration)
    {
        _logger = logger;
        _auditClient = auditClient;
        _auditMapper = auditMapper;
        _serviceConfiguration = serviceConfiguration;
    }

    public async ValueTask<bool> TryAddAsync(ProjectRecord project)
    {
        try
        {
            await _auditClient.AddEntryAsync(new AuditEntry
            {
                ServiceType = ServiceTypes.Agent,
                ServiceUniqueName = _serviceConfiguration.UniqueName,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Type = (int)AuditEntryType.Project,
                AgentProject = _auditMapper.Map(project)
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to add audit entry for project [{projectName}].", project.Meta.Name);
        }

        return false;
    }
}
